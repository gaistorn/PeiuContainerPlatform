using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class SubmitUserStatusModel
    {
        [Display(Name = "userid")]
        [Required(ErrorMessage = "사용자 아이디가 입력되어야 합니다")]
        public string UserId { get; set; }

        [Display(Name = "status")]
        [Required(ErrorMessage = "변경할 상태를 입력해야 합니다")]
        public ContractStatusCodes Status { get; set; }

        [Display(Name = "reason")]
        public string Reason { get; set; }

        [Display(Name = "comment")]
        public string Comment { get; set; }

        [Display(Name = "notification")]
        public bool Notification { get; set; }
    }
}
