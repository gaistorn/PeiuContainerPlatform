using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Hubbub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;
using PeiuPlatform.Model.ExchangeModel;
using PeiuPlatform.Model.IdentityModel;

namespace PeiuPlatform.App.App.Controllers
{
    [Route("api/[controller]/v1")]
    [Authorize]
    [ApiController]
    public class HubbubController : ControllerBase
    {
        //private readonly UserManager<UserAccountEF> _userManager;
        private readonly IMqttPusher mqttPusher;
        private readonly ILogger<HubbubController> logger;
        private readonly ISessionFactory sessionFactory;
        private readonly static Assembly hubbubSharedAssembly = Assembly.GetAssembly(typeof(IModbusHubbubTemplate));
        public HubbubController(ILogger<HubbubController> logger, IMqttPusher mqttPusher, MysqlDataContext dataContext)
        {
            this.mqttPusher = mqttPusher;
            this.logger = logger;

            
            sessionFactory = dataContext.GetSessionFactoryWithAssemblies(hubbubSharedAssembly);
        }

        [HttpGet, Route("controlcheck")]
        public async Task<IActionResult> controlcheck(int siteid)
        {
            bool IsOk = IsControlOk(siteid);
            if (IsOk)
                return Ok();
            else
                return BadRequest();
        }

        private bool IsControlOk(int siteid)
        {
            if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                return true;
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                var claims = HttpContext.User.Claims;
                var enableCtl = claims.FirstOrDefault(x => x.Type == UserClaimTypes.EnableControlBySites);
                if (enableCtl != null)
                {
                    string[] split = enableCtl.Value.Split(',');
                    return split.Contains(siteid.ToString());
                }
            }
            else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                return true;
            }

            return false;
        }

        #region INSERT OR UPDATE API


        /// <summary>
        /// DIGITAL OUTPUT 포인트를 추가/변경 합니다.
        /// ID값이 기존에 있는 경우에는 변경, 없는 경우 추가가 됩니다.
        /// </summary>
        /// <param name="model">DIGITAL OUTPUT 포인트</param>
        /// <returns></returns>
        [HttpPut, Route("modify/digitaloutputpoint")]
        public async Task<IActionResult> InsertDigitalOutputPoint([FromBody] ModbusDigitalOutputPoint model)
        {
            if (ModelState.IsValid)
            {
                return await InsertOrUpdate(model);
            }
            else
            {
                return BadRequest(ApiResult.BAD_REQUEST_400);
            }
        }

        /// <summary>
        /// DIGITAL STATUS 포인트를 추가/변경 합니다.
        /// ID값이 기존에 있는 경우에는 변경, 없는 경우 추가가 됩니다.
        /// </summary>
        /// <param name="model">DIGITAL STATUS 포인트</param>
        /// <returns></returns>
        [HttpPut, Route("modify/digitalstatuspoint")]
        public async Task<IActionResult> InsertDigitalStatusPoint([FromBody] ModbusDigitalStatusPoint model)
        {
            if (ModelState.IsValid)
            {
                return await InsertOrUpdate(model);
            }
            else
            {
                return BadRequest(ApiResult.BAD_REQUEST_400);
            }
        }

        /// <summary>
        /// HUBBUB 정보를 추가/변경 합니다.
        /// ID를 포함하지 않는 경우 추가, 있는 경우 해당 ID의 HUBBUB의 정보를 변경합니다.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("modify/hubbub")]
        public async Task<IActionResult> InsertHubbub([FromBody] ModbusHubbub model)
        {
            if (ModelState.IsValid)
            {
                return await InsertOrUpdate(model);
            }
            else
            {
                return BadRequest(ApiResult.BAD_REQUEST_400);
            }
        }

        /// <summary>
        /// 접속 정보를 추가/변경 합니다.
        /// ID를 포함하지 않는 경우 추가, 있는 경우 해당 ID의 정보를 변경합니다.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("modify/connection")]
        public async Task<IActionResult> InsertConnectionInfo([FromBody] ModbusConnectionInfo model)
        {
            if (ModelState.IsValid)
            {
                return await InsertOrUpdate(model);
            }
            else
            {
                return BadRequest(ApiResult.BAD_REQUEST_400);
            }
        }

        #endregion

        #region REMOVE API
        /// <summary>
        /// 전력수집장치의 Connection 정보를 삭제합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("remove/connection")]
        public async Task<IActionResult> RemoveConnectionInfo(int id)
        {
            if (ModelState.IsValid)
            {
                return await Remove<ModbusConnectionInfo>(id);
            }
            else
            {
                return BadRequest(ApiResult.BAD_REQUEST_400);
            }
        }

        /// <summary>
        /// HUBBUB 정보를 삭제합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("remove/hubbub")]
        public async Task<IActionResult> RemoveHubbub(int id)
        {
            if (ModelState.IsValid)
            {
                return await Remove<ModbusHubbub>(id);
            }
            else
            {
                return BadRequest(ApiResult.BAD_REQUEST_400);
            }
        }
        #endregion

        /// <summary>
        /// 특정 그룹의 MODBUS INPUT 포인트들을 CSV파일로 다운로드 받는다
        /// </summary>
        /// <param name="groupid">포인트 그룹 ID</param>
        /// <returns></returns>
        [HttpGet, Route("download/modbusinputpoint")]
        public async Task<IActionResult> ExportInputPoints(int groupid)
        {
            return await ExportPoints<ModbusInputPoint>(groupid);
        }

        /// <summary>
        /// 특정 그룹의 DIGITAL OUTPUT 포인트들을 CSV 파일로 다운로드 받는다.
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        [HttpGet, Route("download/digitaloutputpoint")]
        public async Task<IActionResult> ExportDigitalOutputPoints(int groupid)
        {
            return await ExportPoints<ModbusDigitalOutputPoint>(groupid);
        }

        /// <summary>
        /// 특정 그룹의 DIGITAL STATUS 포인트들을 CSV 파일로 다운로드 받는다.
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        [HttpGet, Route("download/digitalstatuspoint")]
        public async Task<IActionResult> ExportDigitalStatusPoints(int groupid)
        {
            return await ExportPoints<ModbusDigitalStatusPoint>(groupid);
        }

        private async Task<IActionResult> ExportPoints<T>(int groupid) where T : class
        {
            try

            {
                using (NHibernate.IStatelessSession session = sessionFactory.OpenStatelessSession())
                {
                    //DigitalOutputGroup s;s.
                    var @object = await session.CreateCriteria<T>().Add(Restrictions.Eq("Groupid", groupid)).ListAsync<T>();
                    if (@object == null || @object.Count == 0)
                    {
                        return BadRequest(ApiResult.BadRequest($"대상 GROUP ID {groupid}가 존재하지 않습니다"));
                    }

                    byte[] data = ExportCSVFormat<T>(@object);
                    string filename = $"{typeof(T).Name}.csv";
                    
                    System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                    {
                        
                        FileName = Uri.EscapeUriString(filename),
                        Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
                    };
                    Response.Headers.Add("Content-Disposition", cd.ToString());
                    Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    return File(data, "application/octet-stream");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.BadRequest(ex.Message));
            }
        }

        private byte[] ExportCSVFormat<T>(IEnumerable<T> elements)
        {
            PropertyInfo[] pis = typeof(T).GetProperties();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(',', pis.Select(x => x.Name)));
            foreach (T row in elements)
            {
                List<string> cols = new List<string>();
                foreach(PropertyInfo p in pis)
                {
                    object value = p.GetValue(row);
                    cols.Add(value.ToString());

                }
                sb.AppendLine(string.Join(',', cols));
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private async Task<IActionResult> Remove<T>(int id) where T : class
        {
            try

            {
                using (NHibernate.ISession session = sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    //DigitalOutputGroup s;s.
                    var @object = await session.CreateCriteria<T>().Add(Restrictions.Eq("Id", id)).UniqueResultAsync<T>();
                    if(@object == null)
                    {
                        return BadRequest(ApiResult.BadRequest($"대상 ID {id}가 존재하지 않습니다"));
                    }
                    await session.DeleteAsync(@object);
                    await transaction.CommitAsync();
                    return Ok(ApiResult.OK_200);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ApiResult.BadRequest(ex.Message));
            }
        }

        private async Task<IActionResult> InsertOrUpdate(object @object)
        {
            try
            {
                using (NHibernate.ISession session = sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    await session.SaveOrUpdateAsync(@object);
                    await transaction.CommitAsync();
                }
                return Ok(ApiResult.OK_200);
            }
            catch(Exception ex)
            {
                return BadRequest(ApiResult.BadRequest(ex.Message));
            }

        }

        [HttpGet, Route("pcs_run")]
        public async Task<IActionResult> pcs_run(int siteid, int deviceindex)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);

            model.commandcode = ModbusCommandCodes.RUN;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_stop")]
        public async Task<IActionResult> pcs_stop(int siteid, int deviceindex)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);

            model.commandcode = ModbusCommandCodes.STOP;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_faultreset")]
        public async Task<IActionResult> pcs_faultreset(int siteid, int deviceindex)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);

            model.commandcode = ModbusCommandCodes.RESET;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_emergencystop")]
        public async Task<IActionResult> pcs_emergencystop(int siteid, int deviceindex)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);

            model.commandcode = ModbusCommandCodes.EMERGENCY_STOP;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_manualmode")]
        public async Task<IActionResult> pcs_manualmode(int siteid, int deviceindex)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);

            model.commandcode = ModbusCommandCodes.MANUAL_MODE;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_automode")]
        public async Task<IActionResult> pcs_automode(int siteid, int deviceindex)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);

            model.commandcode = ModbusCommandCodes.AUTO_MODE;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        /// <summary>
        /// 특정 전력수집장치의 포인트 데이터 정보 및 기타 정보를 GZip으로 압축해서 전송
        /// </summary>
        /// <param name="hubbubid">대상 전력수집장치 ID</param>
        /// <param name="compress">GZIP으로 압축해서 전송할지 또는 Json으로 내보낼지<G/param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.HubbubManager)]
        [HttpGet, Route("information/{hubbubid}")]
        public async Task<IActionResult> GetHubbubInformation(int hubbubid, bool compress = false)
        {
            using (IStatelessSession session = sessionFactory.OpenStatelessSession())
            {
                return await GetTemplateCompress(hubbubid, compress, session);
            }
        }

        ///// <summary>
        ///// 특정 사이트 내의 모든 장치의 아날로그 포인트 템플릿을 가져온다. (복수)
        ///// </summary>
        ///// <param name="siteid">대상 사이트 ID</param>
        ///// <returns></returns>
        //[Authorize(Policy = UserPolicyTypes.HubbubManager)]
        //[HttpGet, Route("analogpointtemplate/{siteid}")]
        //public async Task<IActionResult> getanaloginformationBySiteid(int siteid)
        //{
        //    using (IStatelessSession session = sessionFactory.OpenStatelessSession())
        //    {
        //        List<HubbubAnalogMappingTemplate> templates = await GetAnalogTemplateListAsync(siteid, session);
        //        return Ok(templates);
        //    }
        //}

        ///// <summary>
        ///// 특정 사이트의 특정 장치 번호의 아날로그 포인트 템플릿을 가져온다. (단일 1개)
        ///// </summary>
        ///// <param name="siteid">대상 사이트</param>
        ///// <param name="deviceid">대상 장비 ID</param>
        ///// <returns></returns>
        //[Authorize(Policy = UserPolicyTypes.HubbubManager)]
        //[HttpGet, Route("analogpointtemplate/{siteid}/{deviceid}")]
        //public async Task<IActionResult> getanaloginformationByDeviceid(int siteid, int deviceid)
        //{
        //    using (IStatelessSession session = sessionFactory.OpenStatelessSession())
        //    {
        //        HubbubAnalogMappingTemplate templates = await GetAnalogTemplateAsync(siteid, deviceid, session);
        //        if (templates == null)
        //            return BadRequest(ApiResult.BadRequest($"대상 사이트({siteid} 의 장비 ID {deviceid} 를 찾을 수 없습니다"));
        //        return Ok(templates);
        //    }
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteid"></param>
        /// <param name="deviceindex"></param>
        /// <param name="localremote">0:Local, 1:Remote</param>
        /// <returns></returns>
        [HttpGet, Route("pcs_localremote")]
        public async Task<IActionResult> pcs_localremote(int siteid, int deviceindex, ushort localremote)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);
            if (localremote == 0)
                model.commandcode = ModbusCommandCodes.LOCAL_MODE;
            else
                model.commandcode = ModbusCommandCodes.REMOTE_MODE;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            //if(model.LocalRemote == true)
            //{
            //    await Task.Delay(3);
            //    model = CreateModel<PcsControlModel>(siteid, 0, deviceindex);
            //}
            return Ok();
        }


        /// <summary>
        /// ActivePower 제어
        /// </summary>
        /// <param name="siteid"></param>
        /// <param name="deviceindex"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        [HttpGet, Route("pcs_activepower")]
        public async Task<IActionResult> pcs_activepower(int siteid, int deviceindex, float power)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);
            model.commandcode = ModbusCommandCodes.ACTIVE_POWER;
            model.commandvalue = power;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_charging")]
        public async Task<IActionResult> pcs_charging(int siteid, int deviceindex, float power)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);
            model.commandcode = ModbusCommandCodes.CHARGE;
            model.commandvalue = power;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_discharging")]
        public async Task<IActionResult> pcs_discharging(int siteid, int deviceindex, float power)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);
            model.commandcode = ModbusCommandCodes.DISCHARGE;
            model.commandvalue = power;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_socupper")]
        public async Task<IActionResult> pcs_socupper(int siteid, int deviceindex, float socuppper)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);
            model.commandcode = ModbusCommandCodes.LIMIT_SOC_MAX;
            model.commandvalue = socuppper;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        [HttpGet, Route("pcs_soclower")]
        public async Task<IActionResult> pcs_soclower(int siteid, int deviceindex, float soclower)
        {
            if (IsControlOk(siteid) == false)
                return BadRequest();
            ModbusControlModel model = CreateModel<ModbusControlModel>(siteid, 0, deviceindex);
            model.commandcode = ModbusCommandCodes.LIMIT_SOC_MIN;
            model.commandvalue = soclower;
            string topic = $"hubbub/{siteid}/{0}/{deviceindex}/control";

            JObject obj = JObject.FromObject(model);
            await mqttPusher.PushAsync(obj, topic, 2);
            return Ok();
        }

        private async Task<IActionResult> GetTemplateCompress(int hubbubid, bool compress, IStatelessSession session)
        {
            ICriterion expression = Restrictions.Eq("Id", hubbubid);
            var hubbub = await session.CreateCriteria<ModbusHubbub>().
                Add(expression)
                .UniqueResultAsync<ModbusHubbub>();
            if(hubbub == null)
            {
                return BadRequest(ApiResult.BadRequest("존재하지 않는 전력수집장치 ID 입니다"));
            }
            
            ModbusHubbubMappingTemplate row = new ModbusHubbubMappingTemplate();
            row.Hubbub = hubbub;
            row.ConnectionInfo = await GetIdentifiedObjectAsync<ModbusConnectionInfo>(hubbub.Connectionid, session);
            row.ModbusInputPointList = await GetDataByHubbubIdAsync<VwModbusInputPoint>(hubbub.Id, session);
            row.ModbusDigitalOutputPoints = await GetDataByHubbubIdAsync<VwDigitalOutputPoint>(hubbub.Id, session);
            row.ModbusDigitalStatusPoints = await GetDataByHubbubIdAsync<ModbusDigitalStatusPoint>(hubbub.Id, session);
            row.StandardAnalogPoints = await session.CreateCriteria<VwStandardAnalogPoint>()
                .Add(Restrictions.Eq("Disable", (SByte)0))
                .ListAsync<VwStandardAnalogPoint>();
            row.StandardPcsStatuses = await session.CreateCriteria<VwStandardPcsStatusPoint>().ListAsync<VwStandardPcsStatusPoint>();
            //row.AnalogInputPoints = await GetGroupPointsAsync<VwAnalogPoint>(hubbub.Aigroupid, session);
            //row.DigitalInputGroup = await GetIdentifiedObjectAsync<DigitalInputGroup>(hubbub.Digroupid, session);
            //row.DigitalOutputGroup = await GetIdentifiedObjectAsync<DigitalOutputGroup>(hubbub.Dogroupid, session);
            //row.DigitalStatusGroup = await GetIdentifiedObjectAsync<DigitalStatusGroup>(hubbub.Stgroupid, session);
            //row.DigitalInputPoints = await GetGroupPointsAsync<ModbusDigitalInputPoint>(hubbub.Digroupid, session);
            //row.DigitalOutputPoints = await GetGroupPointsAsync<ModbusDigitalOutputPoint>(hubbub.Dogroupid, session);
            //row.DigitalStatusPoints = await GetGroupPointsAsync<ModbusDigitalStatusPoint>(hubbub.Stgroupid, session);
            JObject obj = JObject.FromObject(row);

            if (compress)
            {
                string ori_str = obj.ToString();
                byte[] ori_bytes = Encoding.UTF8.GetBytes(ori_str);
                string ori_str_base64 = Convert.ToBase64String(ori_bytes);

                using (var outputStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        gZipStream.Write(ori_bytes, 0, ori_bytes.Length);
                    }

                    byte[] com_bytes = outputStream.ToArray();
                    string com_str_base64 = Convert.ToBase64String(com_bytes);
                    return Ok(com_str_base64);
                }
            }
            else
            {
                return Ok(row);
            }
        }


        //private async Task<List<HubbubAnalogMappingTemplate>> GetAnalogTemplateListAsync(int siteid, IStatelessSession session)
        //{

        //    ICriterion expression = Restrictions.Eq("Siteid", siteid);
        //    List<HubbubAnalogMappingTemplate> results = new List<HubbubAnalogMappingTemplate>();
        //    var modbusHubbubs = await session.CreateCriteria<ModbusHubbub>().
        //        Add(expression)
        //        .ListAsync<ModbusHubbub>();
        //    foreach (ModbusHubbub modbusHubbub in modbusHubbubs)
        //    {
        //        var template = await GetAnalogModbusHubbubAsync(modbusHubbub, session);
        //        results.Add(template);
        //    }
        //    return results;
        //}

        //private async Task<HubbubAnalogMappingTemplate> GetAnalogTemplateAsync(int siteid, int deviceid, IStatelessSession session)
        //{
        //    ICriterion expression = Restrictions.Eq("Siteid", siteid) && Restrictions.Eq("Deviceindex", deviceid);
        //    var modbusHubbubs = await session.CreateCriteria<ModbusHubbub>().
        //        Add(expression)
        //        .UniqueResultAsync<ModbusHubbub>();
        //    if (modbusHubbubs != null)
        //    {
        //        var template = await GetAnalogModbusHubbubAsync(modbusHubbubs, session);
        //        return template;
        //    }
        //    else return null;
        //}

        //private async Task<HubbubAnalogMappingTemplate> GetAnalogModbusHubbubAsync(ModbusHubbub hubbub, IStatelessSession session)
        //{
        //    HubbubAnalogMappingTemplate hubbubTemplate = new HubbubAnalogMappingTemplate();
        //    hubbubTemplate.Hubbub = hubbub;
        //    hubbubTemplate.ConnectionInfo = await GetIdentifiedObjectAsync<ModbusConnectionInfo>(hubbub.Connectionid, session);
        //    hubbubTemplate.AnalogInputGroup = await GetIdentifiedObjectAsync<AnalogInputGroup>(hubbub.Aigroupid, session);
        //    hubbubTemplate.AnalogInputPoints = await session.CreateCriteria<VwAnalogPoint>().Add(Restrictions.Eq("Groupid", hubbub.Aigroupid)).ListAsync<VwAnalogPoint>();
        //    return hubbubTemplate;
        //}

        private async Task<IList<T>> GetDataByHubbubIdAsync<T>(int Hubbubid, IStatelessSession session) where T : class
        {
            return await session.CreateCriteria<T>().Add(Restrictions.Eq("Hubbubid", Hubbubid)).ListAsync<T>();
        }

        private async Task<T> GetIdentifiedObjectAsync<T>(int id, IStatelessSession session) where T : class
        {
            var modbusHubbubs = await session.CreateCriteria<T>().Add(Restrictions.Eq("Id", id)).UniqueResultAsync<T>();
            return modbusHubbubs;
        }

        private T CreateModel<T>(int siteid, int deviceType, int deviceIndex) where T : ControlModelBase, new()
        {
            T model = new T();
            model.siteid = siteid;
            model.devicetype = deviceType;
            model.deviceindex = deviceIndex;
            var email_claim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "aud");

            model.userid = email_claim != null ? email_claim.Value : "UNKNOWN";
            model.utctimestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            model.localtimestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
            return model;
        }
    }
}