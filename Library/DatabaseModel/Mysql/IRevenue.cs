using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models.Mysql
{
    public interface IRevenue
    {
        int Siteid { get; set; }
        DateTime Createdt { get; set; }
        double Revenue { get; set; }
        int Rcc { get; set; }
    }
}
