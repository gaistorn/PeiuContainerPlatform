using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Notification
{
    public class MMSRequestBodyDetail : MMSRequestBodyBasic
    {
        /// <summary>
        /// 발송 템플릿 ID
        /// </summary>
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [JsonProperty("requestDate")]
        public string RequestDateString
        {
            get
            {
                if (RequestDate.HasValue)
                    return RequestDate.Value.ToString("yyyy-MM-dd HH:mm");
                else
                    return null;
            }
        }

        /// <summary>
        /// 발송 구분자 ex)admin,system
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; } = "Manager";

        /// <summary>
        /// 통계 ID
        /// </summary>
        [JsonProperty("statsId")]
        public string StatsId { get; set; } = "ObznP745";

        /// <summary>
        /// 예약일시 
        /// </summary>
        [JsonIgnore]
        public DateTime? RequestDate { get; set; }

        /// <summary>
        /// 기본적인 SMS 발송 클래스 생성
        /// </summary>
        /// <param name="Title">제목 (120자. 한글 20자, 영문 40자)</param>
        /// <param name="Body">본문 내용(표준 4000바이트. 한글 1000자, 영문 2000자)</param>
        /// <param name="SendNo"><발신 번호/param>
        /// <param name="RecipientNo"><수신 번호/param>
        public MMSRequestBodyDetail(string Title, string Body, string SendNo, params string[] RecipientNo) : base(Title, Body, SendNo, RecipientNo)
        {
        }
    }

    public class MMSRequestBodyBasic
    {
        /// <summary>
        /// 제목 (필수)
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// 본문 내용 (필수)
        /// </summary>
        [JsonProperty("body", Required = Required.Always)]
        public string Body { get; set; }

        /// <summary>
        /// 발신 번호 (필수)
        /// </summary>
        [JsonProperty("sendNo", Required = Required.Always)]
        public string SendNo { get; set; }

        /// <summary>
        /// 수신자 리스트 (필수)
        /// </summary>
        [JsonProperty("recipientList", Required = Required.Always)]
        public List<RecipientBasic> RecipientList { get; set; } = new List<RecipientBasic>();

        /// <summary>
        /// 수신처를 추가한다.
        /// </summary>
        /// <param name="RecipientNo">수신번호</param>
        public void AddRecipientBasic(string RecipientNo)
        {
            RecipientBasic recipientBasic = new RecipientBasic() { RecipientNo = RecipientNo };
            RecipientList.Add(recipientBasic);
        }

        public MMSRequestBodyBasic()
        {
            
        }

        /// <summary>
        /// 기본적인 SMS 발송 클래스 생성
        /// </summary>
        /// <param name="Title">제목 (120자. 한글 20자, 영문 40자)</param>
        /// <param name="Body">본문 내용(표준 4000바이트. 한글 1000자, 영문 2000자)</param>
        /// <param name="SendNo"><발신 번호/param>
        /// <param name="RecipientNo"><수신 번호/param>
        public MMSRequestBodyBasic(string Title, string Body, string SendNo, params string[] RecipientNo)
        {
            this.Title = Title;
            this.Body = Body;
            this.SendNo = SendNo;
            foreach(string no in RecipientNo)
                AddRecipientBasic(no);
        }
    }

    public class RecipientDetail : RecipientBasic
    {
        /// <summary>
        /// 국가 번호 [기본값: 82(한국)]
        /// </summary>
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; } = "82";

        /// <summary>
        /// 국가 번호가 포함된 수신 번호
        /// 예) 82101345678
        /// recipientNo가 있을 경우 이 값은 무시됨
        /// </summary>
        [JsonProperty("internationalRecipientNo")]
        public string InternationalRecipientNo { get; set; }

        /// <summary>
        /// 수신자 그룹키
        /// </summary>
        [JsonProperty("recipientGroupingKey")]
        public string RecipientGroupingKey { get; set; }
    }

    public class RecipientBasic
    {
        /// <summary>
        /// 수신 번호 (필수)
        /// countrycode 국가코드 조합하여 사용 가능
        /// </summary>
        [JsonProperty("recipientNo", Required = Required.Always)]
        public string RecipientNo { get; set; }

    }
}
