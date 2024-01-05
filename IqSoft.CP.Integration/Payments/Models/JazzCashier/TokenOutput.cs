namespace IqSoft.CP.Integration.Payments.Models.JazzCashier
{
    public class TokenOutput
    {
        public Data Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public int IdTypeCode { get; set; }
    }

    public class Data
    {
        public int IdIntegration { get; set; }
        public string Token { get; set; }
    }
}
