using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public abstract class RegisterViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Required]
        [EmailAddress]
        [Display(Name = "email")]
        public string Email { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "firstname")]
        public string FirstName { get; set; }

        //[Required]
        //[StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        [Display(Name = "lastname")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "confirmpassword")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "company")]
        public string Company { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "phonenumber")]
        public string PhoneNumber { get; set; }

        [Display(Name = "address")]
        public string Address { get; set; }

        //[Display(Name = "type")]
        //[JsonConverter(typeof(StringEnumConverter))]
        //public  RegisterType Type { get; set; }

        [Display(Name = "registernumber")]
        [Required(ErrorMessage = "사업자 등록 번호를 입력하세요")]
        public string RegisterNumber { get; set; }

        [Display(Name = "registerfilename")]
        [Required(ErrorMessage = "사업자 등록증을 첨부해주세요")]
        public string RegisterFilename { get; set; }

        [Display(Name = "registerfilebase64")]
        [Required(ErrorMessage = "사업자 등록증을 첨부해주세요")]
        public string RegisterFilebase64 { get; set; }

    }

    public class AggregatorGroupRegistModel
    {
        [Required]
        [Display(Name = "name")]
        public string AggregatorGroupName { get; set; }


        /// <summary>
        /// 상호 또는 대표 이름
        /// </summary>
        [Required]
        [Display(Name = "representation")]
        
        public string Represenation { get; set; }

        [Display(Name = "address")]
        public string Address { get; set; }

        [Display(Name = "phonenumber")]
        public string PhoneNumber { get; set; }



    }

    public class AggregatorRegistModelBase : RegisterViewModel
    {
        [Display(Name = "aggregatorgroupid")]
        public string AggregatorGroupId { get; set; }

        public bool NotifyEmail { get; set; }
    }
}
