using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public class StatusValue : ValueTemplate
    {
        public override ValuesCategory Category => ValuesCategory.Status;
        public EventValue[] Faults;
        public EventValue[] Warrning;
        public EventValue[] Status;
    }
}
