namespace IqSoft.CP.Common.Models
{
    public class XeRateResponse
    {
        public string Timestamp { get; set; }

        public Rate[] To { get; set; }
    }

    public class Rate
    {
        public string QuoteCurrency { get; set; }

        public decimal Mid { get; set; }
    }

    public class AuthResponse
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public string Documentation_Url { get; set; }
    }
}
