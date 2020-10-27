using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Notification
{
    public class MessageResponse
    {
        [JsonProperty("header")]
        public ResponseHeader Header { get; set; }

        [JsonProperty("body")]
        public ResponseBody Body { get; set; }
    }

    public class ResponseHeader
    {
        /// <summary>
        /// 성공 여부
        /// </summary>
        [JsonProperty("isSuccessful")]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// 실패 코드
        /// </summary>
        [JsonProperty("resultCode")]
        public int ResultCode { get; set; }

        /// <summary>
        /// 실패 메시지
        /// </summary>
        [JsonProperty("resultMessage")]
        public string ResultMessage { get; set; }
    }

    public class ResponseBody
    {
        [JsonProperty("data")]
        public ResponseBodyData Data { get; set; }
    }

    public class ResponseBodyData
    {
        /// <summary>
        /// 요청 ID
        /// </summary>
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        /// <summary>
        /// 요청 상태 코드(1:요청중, 2:요청 완료, 3:요청 실패)
        /// </summary>
        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }

        /// <summary>
        /// 발신자 그룹키
        /// </summary>
        [JsonProperty("senderGroupingKey")]
        public string SenderGroupingKey { get; set; }

        [JsonProperty("sendResultList")]
        public ResponseBodyDataSendResult[] SendResultList { get; set; }
    }

    public class ResponseBodyDataSendResult
    {
        /// <summary>
        /// 수신 번호
        /// </summary>
        [JsonProperty("recipientNo")]
        public string RecipientNo { get; set; }

        /// <summary>
        /// 결과 코드
        /// </summary>
        [JsonProperty("resultCode")]
        public int ResultCode { get; set; }

        /// <summary>
        /// 결과 메시지
        /// </summary>
        [JsonProperty("resultMessage")]
        public string ResultMessage { get; set; }

        /// <summary>
        /// 수신자 시퀀스
        /// </summary>
        [JsonProperty("recipientSeq")]
        public int RecipientSeq { get; set; }

        [JsonProperty("recipientGroupingKey")]
        public string RecipientGroupingKey { get; set; }
    }
}
