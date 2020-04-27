using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public abstract class MeasurementValue : ValueTemplate
    {
        public override ValuesCategory Category => ValuesCategory.Measurement;
    }
}
