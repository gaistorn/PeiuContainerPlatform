using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace Hubbub
{
    public static class ZkFactory
    {
        private static LoggedWatcher watcher;
        private static ZooKeeper zooKeeper;
        private static HubbubReport report;
        private static bool IsInitializeReport = false;
        static ILogger logger;
        const int CONNECTION_TIMEOUT = 4000;

        public static void Initialize(ILogger Logger)
        {
            logger = Logger;
            watcher = new LoggedWatcher(Logger);
        }

        private static string GetNodeName(int siteid, int deviceindex)
        {
            return $"/hubbub/{siteid}-{deviceindex}";
        }

        private static HubbubReport GetHubbubReport(int siteid, int deviceid, enStatus status = enStatus.RUN)
        {
            if(IsInitializeReport == false)
            {
                report = new HubbubReport();
                report.iSiteId = siteid;
                report.iDeviceIndex = deviceid;
                report.strHost = Dns.GetHostName();
                report.strIp = string.Join(',', Dns.GetHostEntry(report.strHost).AddressList.Select(x => x.ToString()));
                report.strOSVer = Environment.OSVersion.ToString();
            }
            report.Status = status;
            return report;
        }

        private static ConcurrentDictionary<int, enStatus> lastZkStatus = new ConcurrentDictionary<int, enStatus>();

        public static async Task ReportHubbubStatusAsync(string ZkServerHost, int siteid, int deviceid, enStatus status, CancellationToken cancellationToken)
        {
            if (HubbubInformation.GlobalHubbubInformation.use_zookeeper == false)
                return;
            try
            {
                bool IsNotConnected = zooKeeper == null || zooKeeper.getState() == ZooKeeper.States.NOT_CONNECTED || zooKeeper.getState() == ZooKeeper.States.CLOSED;
                HubbubReport report = GetHubbubReport(siteid, deviceid, status);
                byte[] data = report.ToByteArray();
                if (IsNotConnected)
                {
                    zooKeeper = new ZooKeeper(ZkServerHost, HubbubInformation.GlobalHubbubInformation.zookeeper_timeout, watcher);
                    lastZkStatus.Clear();
                }

                enStatus enStatus = enStatus.NO_STATUS;
                if (lastZkStatus.TryGetValue(deviceid, out enStatus) && enStatus == status)
                {
                    return;
                }

                lastZkStatus.AddOrUpdate(
                    deviceid,
                    enStatus,
                    (oriStatus, existStatus) =>
                    {
                        return existStatus;
                    });
                
                string zkName = GetNodeName(siteid, deviceid);
                var existState = await zooKeeper.existsAsync(zkName, true);
                if (existState == null)
                {
                    await zooKeeper.createAsync(zkName, data, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
                    Console.WriteLine("write zk:" + zkName);
                }
                else
                {
                    await zooKeeper.setDataAsync(zkName, data);
                    Console.WriteLine("update zk:" + zkName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                await Task.Delay(5000);
            }
            finally
            {
            }
        }
    }
}
