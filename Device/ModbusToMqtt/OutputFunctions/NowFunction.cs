using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Power21.Device
{
    public class NowFunction : OutputFunction
    {
        public override string Execute(string Parameter)
        {
            if(string.IsNullOrEmpty(Parameter))
            {
                return DateTime.Now.ToString();
            }
            else
            {
                return DateTime.Now.ToString(Parameter);
            }
        }

        public override bool Validation(string functionName, string Parameter)
        {
            return string.IsNullOrEmpty(functionName) == false && functionName.ToLower().StartsWith("@now");
        }
    }
}
