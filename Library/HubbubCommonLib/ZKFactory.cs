using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using PeiuPlatform.Hubbub.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    public interface IZKFactory : IDisposable
    {
        Task WaitWorking(CancellationToken cancellationToken);
        Task Waiting(CancellationToken cancellationToken);
        Task ReportDeviceStatus(int index);
    }
    public class ZKFactory : IZKFactory
    {
        private readonly ZooKeeper zooKeeper;
        private readonly ILogger<ZKFactory> logger;
        private readonly LoggedWatcher watcher;
        private readonly int siteId;
        const int CONNECTION_TIMEOUT = 4000;
        private bool RunningWorker = false;
        private readonly TimeSpan FORCED_TIMEOUT = TimeSpan.FromMinutes(5);

        public ZKFactory(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            siteId = int.Parse(configuration.GetSection("hubbub:siteid").Value);
            string zkServer = configuration.GetConnectionString("zkserver");
            if (string.IsNullOrEmpty(zkServer))
                zkServer = "www.peiu.co.kr:3090";
            logger = loggerFactory.CreateLogger<ZKFactory>();
            watcher = new LoggedWatcher(loggerFactory.CreateLogger<LoggedWatcher>());
            zooKeeper = new ZooKeeper("www.peiu.co.kr:3090", CONNECTION_TIMEOUT, watcher);
        }

        private string GetZKName(int deviceidx = -1)
        {
            if(deviceidx >=0)
                return $"/hubbub/{siteId}/{deviceidx}";
            else
                return $"/hubbub/{siteId}";
        }

        public void Dispose()
        {
                
        }

        public async Task ReportStatus()
        {
            ZKHubbub report = new ZKHubbub();
            report.lShutonUtcUnixTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            report.strHost = Dns.GetHostName();
            report.strIp = Dns.GetHostEntry(report.strHost).AddressList[0].ToString();
            report.strOSVer = Environment.OSVersion.ToString();

            var existState = await zooKeeper.existsAsync(GetZKName());
            if (existState == null)
                await zooKeeper.createAsync(GetZKName(), report.ToByteArray(), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
            else
                await zooKeeper.setDataAsync(GetZKName(), report.ToByteArray());
        }

        public async Task Waiting(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested || RunningWorker == false)
            {
                if (RunningWorker)
                    break;
                await Task.Delay(500, cancellationToken);
            }
        }


        public async Task WaitWorking(CancellationToken cancellationToken)
        {
            bool IsConnected = false;
            DateTime FORCED_TIME = DateTime.Now.Add(FORCED_TIMEOUT);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var status = zooKeeper.getState();
                    if (status == ZooKeeper.States.CONNECTED)
                    {
                        if (IsConnected == false)
                        {
                            Console.WriteLine("Success to connect zk server.");
                            IsConnected = true;
                        }
                        await Task.Delay(1000, cancellationToken);
                        var existState = await zooKeeper.existsAsync(GetZKName(), true);
                        if (existState == null)
                        {
                            await ReportStatus();
                            RunningWorker = true;
                            break;
                        }
                    }
                    else
                    {
                        if (FORCED_TIME <= DateTime.Now)
                        {
                            // 주키퍼 서버에 접속할 수 없어서 강제적으로 실행합니다.
                            // 이부분은 보류
                            //break;
                        }
                    }
                    await Task.Delay(1000, cancellationToken);
                    //var stat = await zk.existsAsync("/master", liveWatcher);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task ReportDeviceStatus(int index)
        {
            ZKDevice device = new ZKDevice();
            device.lLastOperateUtcUnixtimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await zooKeeper.setDataAsync(GetZKName(index), device.ToByteArray());
        }
    }
}
