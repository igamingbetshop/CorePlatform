namespace IqSoft.CP.Integration.Payments.Models.PaymentAsia
{
    public class PayoutOutput
    {
        public Request request { get; set; }
        public Response response { get; set; }
        //public Payload payload { get; set; }
    }

    public class Request
    {
        public string id { get; set; }
        public string time { get; set; }
    }

    public class Response
    {
        public int code { get; set; }
        public string message { get; set; }
        public string time { get; set; }
    }

    public class Payload
    {
        public string request_reference { get; set; }
        public string status { get; set; }
    }
}