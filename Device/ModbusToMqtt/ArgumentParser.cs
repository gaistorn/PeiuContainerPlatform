using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Power21.Device
{
    public class ArgumentParser
    {
        public string LookupServer { get; set; }
        public ushort HeartBeatServerPort { get; set; } = 12345;

        public int TimeoutMS { get; set; } = 3000;

        private static readonly string[] LookupArguments = { "-l", "--lookup" };
        private static readonly string[] PortArguments = { "-p", "--port" };
        private static readonly string[] TimeoutArguments = { "-t", "--timeout" };
        private ArgumentParser()
        {

        }

        public static bool TryParse(string[] args, out ArgumentParser argument)
        {
            argument = new ArgumentParser();
            int lookupIdx = IndexOf(args, LookupArguments);
            if (lookupIdx < 0)
                return false;
            
            argument.LookupServer = args[lookupIdx + 1];
            int portIdx = IndexOf(args, PortArguments);
            if (portIdx >= 0)
                argument.HeartBeatServerPort = ushort.Parse(args[portIdx + 1]);
            int timeoutIdx = IndexOf(args, TimeoutArguments);
            if (timeoutIdx >= 0)
                argument.TimeoutMS = int.Parse(args[timeoutIdx + 1]);
            return true;
        }

        private static int IndexOf(string[] args, string[] argues)
        {
            foreach(string arg in argues)
            {
                int idx = Array.IndexOf(args, arg);
                if (idx >= 0)
                    return idx;
            }
            return -1;
        }

        public static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: " + Assembly.GetExecutingAssembly().GetName().Name + " [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine($"  {string.Join('|', LookupArguments)} [bind_address]\t\tTarget lookup bind address (iec, tcp://127.0.0.1:12345)");
            Console.WriteLine($"  {string.Join('|', PortArguments)} [port]\t\t");
            Console.WriteLine($"  {string.Join('|', TimeoutArguments)} [timeout ms]\t\t");
        }
    }
}
