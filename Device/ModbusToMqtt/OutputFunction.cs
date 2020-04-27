using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Power21.Device
{
    public abstract class OutputFunction
    {
        public abstract bool Validation(string functionName, string Parameter);
        public abstract string Execute(string Parameter);

    }

    public static class OutputFunctionFactory
    {
        static OutputFunction[] funcs =
        {
            new NowFunction()
        };

        public static bool ValidateAndExecute(string fieldName, out string Value)
        {
            Value = null;
            if (fieldName.StartsWith('@') == false)
            {
                return false;
            }

            string[] splits = fieldName.Split(':');
            string funcName = splits[0];
            string parameter = splits.Length > 1 ? splits[1] : null;

            foreach(OutputFunction fc in funcs)
            {
                if(fc.Validation(funcName, parameter))
                {
                    Value = fc.Execute(parameter);
                    return true;
                }
            }
            return false;
        }
    }
}
