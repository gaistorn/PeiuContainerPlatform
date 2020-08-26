using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using org.apache.zookeeper;
using org.apache.utils;
using System.Threading;

namespace PongServer
{
    public class Program
    {
        readonly static CancellationTokenSource CancelWaitSource;
        static Program()
        {
            CancelWaitSource = new CancellationTokenSource();
        }
        const int CONNECTION_TIMEOUT = 4000;
        public static void Main(string[] args)
        {
            ushort b = 8 | 512;
            byte[] bits = BitConverter.GetBytes(b);
            Task T = CreateClient(CancelWaitSource.Token);
            T.Wait();
            CreateHostBuilder(args).Build().Run();
        }

        
       private static async Task CreateClient(CancellationToken token)
        {
            LiveWatcher liveWatcher = new LiveWatcher();
            ZooKeeper zk = new ZooKeeper("www.peiu.co.kr:3090", CONNECTION_TIMEOUT, liveWatcher);
            DateTime Timeout = DateTime.Now.AddMinutes(5);
            Console.WriteLine("Connecting zk server...");
            bool IsConnected = false;
            while (!token.IsCancellationRequested)
            {
                var status = zk.getState();   
                if(status == ZooKeeper.States.CONNECTED)
                {
                    if (IsConnected == false)
                    {
                        Console.WriteLine("Success to connect zk server.");
                        IsConnected = true;
                    }

                    var existState = await zk.existsAsync("/hubbub/160");
                    if (existState == null)
                    {
                        await zk.createAsync("/hubbub/160", BitConverter.GetBytes(666), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
                        break;
                        //Environment.OSVersion
                        
                    }
                    else
                    {

                    }
                    
                    
                }
                else
                {
                    if(Timeout <= DateTime.Now)
                    {
                        // 주키퍼 서버에 접속할 수 없어서 강제적으로 실행합니다.
                        break;
                    }
                }
                await Task.Delay(1000, token);
                //var stat = await zk.existsAsync("/master", liveWatcher);
            }
            //var stat = await zk.existsAsync("/master", liveWatcher);
            //if (stat == null)
            //    break;
            //DataResult dataresult = await zk.getDataAsync("/master", liveWatcher);
            ////await Task.Delay(CONNECTION_TIMEOUT);
            //ChildrenResult result = await zk.getChildrenAsync("/", liveWatcher);
            //Console.WriteLine($"[{string.Join(',', result.Children)}]");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddHostedService<DummyWorker>();
                    ExecuteArguments ar = new ExecuteArguments(args);
                    services.AddSingleton(ar);
                    services.AddHostedService<Worker>();
                });
    }

    internal class LiveWatcher : Watcher
    {
        public override Task process(WatchedEvent @event)
        {
            Console.WriteLine($"event type: {@event.get_Type()}, Path={@event.getPath()} State={@event.getState()}");
            
            return Task.CompletedTask;
        }
    }
}
