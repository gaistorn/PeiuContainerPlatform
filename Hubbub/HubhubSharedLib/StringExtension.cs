using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hubbub
{
    public static class StringExtension
    {
        const string interpolatePattern = @"{(.+?)}";
        public static string Interpolate(this string template, params Expression<Func<object, object>>[] values)
        {
            string result = template;
            if (Regex.IsMatch(template, interpolatePattern))
            {
                Match mc = Regex.Match(template, interpolatePattern);
                string member = mc.Value.TrimStart('{').TrimEnd('}');

                values.ToList().ForEach(x =>
                {
                    //if(x.Parameters.pa
                    if(x.Parameters.Any(x=>x.Name == member))
                    {
                        string value = x.Compile().Invoke(null).ToString();
                        result = Regex.Replace(template, interpolatePattern, value);

                    }
                });

                //string result = Regex.Replace(template, interpolatePattern, (dynamic)@object.member)
            }
            return result;
        }
    }
}
