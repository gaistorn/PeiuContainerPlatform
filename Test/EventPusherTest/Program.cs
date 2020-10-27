using FireworksFramework.Mqtt;
using PeiuPlatform.App;
using PeiuPlatform.Lib;
using System;
using System.IO.Ports;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using PeiuPlatform.Notification;

namespace EventPusherTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            int caseNo = 0;

            switch(caseNo)
            {
                case 0:
                    Console.WriteLine("0");
                    break;
                case 1:
                    Console.WriteLine("1");
                    break;
                case 3:
                    break;
            }

            Notificator notificator = new Notificator("4ERp0UUnWCuDzwnv");

            Task t_sms = notificator.SendingMMS("제목", "Test M\nInline", "01063671293", "01063671293");
            t_sms.Wait();


            ConcurrentDictionary<int, int> keyValuePairs = new ConcurrentDictionary<int, int>();
            keyValuePairs.AddOrUpdate(3, 10, (newValue, oldValue) =>
            {
                return oldValue;
            });
            int nValue = 20;
            bool IsChanged = false;
            int rValue = keyValuePairs.AddOrUpdate(3, nValue, (key, oldValue) =>
            {
                IsChanged = true;
                return nValue;
            });

            string jobStr = "{Name} {id} job for admin";
            var d = new { Id = 1, Name = "Todo", Description = "Nothing" };
            string id = "1234";
            var result = jobStr.Interpolate(x => d.Name, x=> id);

            string sdf = "WdR";
            int ascii = Convert.ToInt32(sdf[0]);
            ascii = ascii + 32;
            Char c = Convert.ToChar(ascii);

            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            EventPublisherWorker worker = new EventPublisherWorker();
            worker.Initialize();
            CancellationTokenSource tk = new CancellationTokenSource();
            while (true)
            {
                Console.Write("Input the Event Code: ");
                string code = Console.ReadLine();
                if (ushort.TryParse(code, out ushort EventCode))
                {
                    Task t = worker.UpdateDigitalPoint(6, DeviceTypes.BMS, 1, 1, 40132, EventCode, tk.Token);
                    t.Wait();
                }
            }
        }

       
    }

    public static class StringExtension
    {
        public static string Interpolate(this string template, params Expression<Func<object, string>>[] values)
        {
            string result = template;
            values.ToList().ForEach(x =>
            {
                MemberExpression member = x.Body as MemberExpression;
                string oldValue = $"{{{member.Member.Name}}}";
                string newValue = x.Compile().Invoke(null).ToString();
                result = result.Replace(oldValue, newValue);
            }

                );
            return result;
        }
    }
}
