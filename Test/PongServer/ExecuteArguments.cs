using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace PongServer
{
    public class ExecuteArguments
    {
        public string LookupServerAddress { get; set; }
        public ushort HostPort { get; set; }

        public ExecuteArguments(string[] args)
        {
            int lookupIndex = Array.IndexOf(args, "-l");
            if (lookupIndex < 0)
                throw new InvalidOperationException();
            LookupServerAddress = args[lookupIndex + 1];
            int portIdx = Array.IndexOf(args, "-p");
            if(portIdx >= 0)
            {
                HostPort = ushort.Parse(args[portIdx + 1]);
            }
        }
    }
}
