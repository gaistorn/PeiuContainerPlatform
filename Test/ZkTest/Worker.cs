using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;

namespace ZkTest
{
    public class Worker : BackgroundService
    {
        private static int WorkerCount = 0;
        private int workerid;
        private readonly ILogger<Worker> _logger;
        private readonly LoggedWatcher watcher;
        private ZooKeeper zooKeeper;
        const int CONNECTION_TIMEOUT = 4000;
        const string zkServer = "www.peiu.co.kr:3090";
        private readonly IHostApplicationLifetime lifeTime;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            lifeTime = hostApplicationLifetime;

            watcher = new LoggedWatcher(logger);
            //zooKeeper = new ZooKeeper(zkServer, CONNECTION_TIMEOUT, watcher);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //workerid = Interlocked.Increment(ref WorkerCount);
            string inputstr = "Hello World";
            byte[] inputbytes = Encoding.UTF8.GetBytes(inputstr);
            using(var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(inputbytes, 0, inputbytes.Length);
                byte[] compressBytes = outputStream.ToArray();
                var outputStr = Encoding.UTF8.GetString(compressBytes);
            }

            _logger.LogInformation($"Worker ID: {workerid} running at: {DateTimeOffset.Now}");
            Task[] tasks = new Task[]
            {
                CreateTask(TimeSpan.FromSeconds(5), lifeTime.ApplicationStopping),
                CreateTask(TimeSpan.FromSeconds(2), lifeTime.ApplicationStopping),
                CreateTask(TimeSpan.FromSeconds(13), lifeTime.ApplicationStopping),
                CreateTask(TimeSpan.FromSeconds(10), lifeTime.ApplicationStopping),
                CreateTask(TimeSpan.FromSeconds(4), lifeTime.ApplicationStopping),
                CreateTask(TimeSpan.FromSeconds(1), lifeTime.ApplicationStopping),
            };

            Task.WaitAll(tasks);

            

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    await TryConnect(stoppingToken);
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }

        private async Task CreateTask(TimeSpan ts, CancellationToken cancellationToken)
        {
            int id = Interlocked.Increment(ref WorkerCount);
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker ID: {id} running at: {DateTimeOffset.Now} delay: {ts}");
                await Task.Delay(ts);
            }
            _logger.LogWarning($"Worker ID: {id} ABORT at: {DateTimeOffset.Now}");
        }

        private async Task TryConnect(CancellationToken cancellationToken)
        {
            try
            {
                bool IsNotConnected = zooKeeper == null || zooKeeper.getState() == ZooKeeper.States.NOT_CONNECTED || zooKeeper.getState() == ZooKeeper.States.CLOSED;
                if (IsNotConnected)
                {
                    zooKeeper = new ZooKeeper(zkServer, CONNECTION_TIMEOUT, watcher);
                    string zkName = GetZKName();
                    
                    var existState = await zooKeeper.existsAsync(zkName, true);
                    if (existState == null)
                    {
                        byte[] data = BitConverter.GetBytes(666);
                        await zooKeeper.createAsync(zkName, data, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
            }
        }

        private string GetZKName()
        {
            return $"/hubbub/6";
        }
    }
}
