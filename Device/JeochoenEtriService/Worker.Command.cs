using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using NModbus;

namespace PeiuPlatform.Hubbub
{
    partial class Worker
    {
        private DateTime LastReadingCommandDate;
        private async void RunCommand(ISession session, IModbusMaster master, CancellationToken stoppingToken)
        {
            DateTime startDate = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
            var commandLists = await GetCommandList(session);
            if (commandLists == null)
                return;
            VwCurrentcommand command = null;

            if (commandLists.Count > 0)
            {
                command = commandLists.FirstOrDefault();
            }
            else
            {

                await SOCProtectFunction(master, stoppingToken);
                return;
            }

            
            if (command == null || (command.Date == LastReadingCommandDate.Date && command.Hour == LastReadingCommandDate.Hour && command.Minute == LastReadingCommandDate.Minute))
            {
                /// 제어 명령이 없으면 SOC 프로텍션 기능 실행
                await SOCProtectFunction(master, stoppingToken);
                return;
            }

            LastReadingCommandDate = command.Date.AddHours(command.Hour).AddMinutes(command.Minute);

            _logger.LogWarning($"command pcs1: {command.Ess1} pcs2: {command.Ess2} pcs3: {command.Ess3} pcs4: {command.Ess4}");
            /// 1단계 SOC 내의 명령인지 체크
            bool[] ValidSocs = new bool[]
            {
                            await ValidatingSOC(1, command.Ess1),
                            await ValidatingSOC(2, command.Ess2),
                            await ValidatingSOC(3, command.Ess3),
                            await ValidatingSOC(4, command.Ess4)
            };


            bool pcs1_ok = ValidSocs[0];
            bool pcs2_ok = ValidSocs[1];
            bool pcs3_ok = ValidSocs[2];
            bool pcs4_ok = ValidSocs[3];


            await SendCommandWhenOK(ValidSocs[0], 1, command.Ess1, master);
            await SendCommandWhenOK(ValidSocs[1], 2, command.Ess2, master);
            await SendCommandWhenOK(ValidSocs[2], 3, command.Ess3, master);
            await SendCommandWhenOK(ValidSocs[3], 4, command.Ess4, master);
            await Task.Delay(1000, stoppingToken);
        }

        private async Task SendCommandWhenOK(bool IsOK, int pcsNo, float etri_command, IModbusMaster master)
        {
            try
            {
                if (IsOK == false)
                {
                    //await Task.CompletedTask;
                    return;
                }
                float cmdValue = await ValidatingPV(1, etri_command);
                ushort cmdUshort = (ushort)(cmdValue * 10);
                ushort startAddress = (ushort)(600 + ((pcsNo - 1) * 200));
                
                await master.WriteMultipleRegistersAsync(1, (ushort)(startAddress + 1), new ushort[] { cmdUshort });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task SOCProtectFunction(IModbusMaster master, CancellationToken stoppingToken)
        {
            for (int pcsNo = 1; pcsNo <= 4; pcsNo++)
            {
                if (await GetManualAuto(pcsNo) == 0)
                    continue;
                float soc = await GetSOC(pcsNo);
                float[] soc_min_max = await GetSOCMINMAX(pcsNo);
                float soc_min = soc_min_max[0];
                if (soc < soc_min)
                {
                    float cmdValue = -1;
                    ushort cmdUshort = (ushort)(cmdValue * 10);
                    
                    _logger.LogWarning($"[경고] [PCS{pcsNo}] 현재 SOC({soc})가 SOC MIN({soc_min}) 미만으로 되었습니다. 강제로 충전 명령({-1})을 실행합니다");
                    await SendCommandWhenOK(true, pcsNo, -1, master);
                }
            }
        }

        //private async Task<bool> AutoModeCheck(int PcsNo)
        //{
        //    var flag = await GetLocalRemote(PcsNo);
        //    return flag == 1;

        //}

        private async Task<float> ValidatingPV(int PcsNo, float Command)
        {
            float pv_power = await GetPVPower(PcsNo);

            // 방전명령일 경우
            if (Command > 0)
            {
                float MinTarget = Math.Min(Command, (94 - pv_power));
                return MinTarget;
            }
            else if (Command < 0) // 충전명령 일 경우
            {
                float MaxTarget = Math.Max(Command, (pv_power * -1));
                return MaxTarget;
            }
            else return 0;
        }

        private async Task<bool> ValidatingSOC(int PcsNo, float Command)
        {
            float soc = await GetSOC(PcsNo);
            float[] soc_min_max = await GetSOCMINMAX(PcsNo);
            float soc_min = soc_min_max[0];
            float soc_max = soc_min_max[1];

            if (await GetManualAuto(PcsNo) == 0)
            {
                _logger.LogWarning($"[경고] [PCS{PcsNo}] 현재 Manual 모드입니다");
                return false;
            }

            if (Command < 0 && soc > soc_max)
            {
                _logger.LogWarning($"[경고] [PCS{PcsNo}] 현재 방전 명령({Command})이 취소되었습니다. 사유) SOC({soc})가 최대 SOC범위 ({soc_max})를 초과했습니다");
                return false;
            }
            else if (Command > 0 && soc < soc_min)
            {
                _logger.LogWarning($"[경고] [PCS{PcsNo}] 현재 충전 명령({Command})이 취소되었습니다. 사유) SOC({soc})가 최소 SOC범위 ({soc_min})를 미만입니다");
                return false;
            }
            return true;
        }

        private TimeSpan GetPureTime(TimeSpan ts)
        {
            return new TimeSpan(ts.Hours, ts.Minutes, 0);
        }

        private async Task<IList<VwCurrentcommand>> GetCommandList(ISession session)
        {
            try
            {
                DateTime startDate = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
                var commandLists = await session.CreateCriteria<VwCurrentcommand>()
                    .Add(Restrictions.Eq("Date", DateTime.Now.Date) && Restrictions.Eq("Hour", DateTime.Now.Hour) && Restrictions.Eq("Minute", DateTime.Now.Minute))
                    .AddOrder(NHibernate.Criterion.Order.Desc("Date"))
                    .ListAsync<VwCurrentcommand>();
                return commandLists;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<float> GetPVPower(int No)
        {
            string redisKey = $"6.JeJuGridPcs{No}";
            float fValue = (float)await _dataAccessor.GetValue<float>(redisKey, "ID_POWER");
            return fValue;
        }
               
        private async Task<float> GetSOC(int No)
        {
            string redisKey = $"BSC#{No}.SOC";
            float fValue = (float)await _dataAccessor.GetValue<float>(redisKey);
            return fValue;
        }

        private async Task<float[]> GetSOCMINMAX(int No)
        {
            string mnKey = $"PCS#{No}.SocMin";
            string mxKey = $"PCS#{No}.SocMax";
            
            float mnValue = (float)await _dataAccessor.GetValue<float>(mnKey);
            float mxValue = (float)await _dataAccessor.GetValue<float>(mxKey);
            return new float[] { mnValue, mxValue };
        }

        private async Task<float> GetPCSValues(int PcsNo, RedisValue FieldName)
        {
            string redisKey = $"PCS#{PcsNo}.{FieldName}";
            float fValue = (float)await _dataAccessor.GetValue<float>(redisKey);

            return fValue;
        }

        private async Task<int> GetManualAuto(int PcsNo)
        {
            string redisKey = $"PCS#{PcsNo}.ManualAuto";
            var results = await _dataAccessor.GetValue<int>(redisKey);
            //return (int)results;
            return results;
            //var results = await redisdatabaseAsync.HashGetAsync(redisKey, "LocalRemote");
            //return (int)results;
        }

        //private async Task<int> GetOperateStatus(int PcsNo, OperateProperty property)
        //{
        //    string redisKey = $"JeJuGridPcs{PcsNo}.40181.{(int)property}";
        //    int v = (int)await redisdatabaseAsync.HashGetAsync(redisKey, "Status");
        //    return v;

        //}
    }
}
