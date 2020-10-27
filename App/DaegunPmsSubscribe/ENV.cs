using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform
{
    public class ENV
    {
        public static int SiteId => int.Parse(Environment.GetEnvironmentVariable("ENV_SITE_ID"));
        public static int RccId => int.Parse(Environment.GetEnvironmentVariable("ENV_RCC_ID"));
        public static int PcsCount => int.Parse(Environment.GetEnvironmentVariable("ENV_PCS_COUNT"));
    }
}
