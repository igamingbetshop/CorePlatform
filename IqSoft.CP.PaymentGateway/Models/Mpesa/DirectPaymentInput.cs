using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Mpesa
{
    public class DirectPaymentInput
    {
        public ResultModel Result { get; set; }
    }

    public class ResultModel
    {
        public string ResultType { get; set; }
        public int ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public string OriginatorConversationID { get; set; }
        public string ConversationID { get; set; }
        public string TransactionID { get; set; }
        public List<ResultItem> ResultParameters { get; set; }
    }

    public class ResultItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}