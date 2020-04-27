using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class RegisterSiteModel
    {
        public RegisterSiteModel() { }

        [Required]
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }

        [Required]
        [EmailAddress]
        public virtual string ContractorEmail { get; set; }


        public virtual double Latitude { get; set; }

        public virtual double Longtidue { get; set; }

        public virtual ServiceCodes ServiceCode { get; set; }

        public virtual string LawFirstCode { get; set; }

        public virtual string LawMiddleCode { get; set; }

        public virtual string LawLastCode { get; set; }

        public virtual bool IsJejuSite { get; set; }

        [Display(Name = "assets")]
        [Required(ErrorMessage = "반드시 자산을 입력해야 합니다")]
        public RegisterAssetModel[] Assets { get; set; }
    }

    public class RegisterSiteModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
