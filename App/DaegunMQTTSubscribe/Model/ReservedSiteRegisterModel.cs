using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Power21.PEIUEcosystem.Models
{
    public class ReservedSiteRegisterModel
    {
        [Required]
        [Display(Name = "address1")]
        public virtual string Address1 { get; set; }

        [Display(Name = "address2")]
        public virtual string Address2 { get; set; }

        [Required]
        [Display(Name = "accountid")]
        public virtual string AccountId { get; set; }

        [Required]
        [Display(Name = "controlowner")]
        public virtual bool ControlOwner { get; set; }

        [Display(Name = "latitude")]
        public virtual double Latitude { get; set; }

        [Display(Name = "longtidue")]
        public virtual double Longtidue { get; set; }

        [Display(Name = "servicecode")]
        public virtual int ServiceCode { get; set; }

        [Display(Name = "lawfirstcode")]
        public virtual string LawFirstCode { get; set; }

        [Display(Name = "lawmiddlecode")]
        public virtual string LawMiddleCode { get; set; }

        [Display(Name = "lawlastcode")]
        public virtual string LawLastCode { get; set; }

        [Display(Name = "devices")]
        public List<SimpleDeviceModel> Devices { get; set; }
    }

    public class SimpleDeviceModel
    {
        public int DeviceType { get; set; }
        public string DeviceName { get; set; }
        public double Amount { get; set; }
    }
}
