namespace IqSoft.CP.ProductGateway.Models.SunCity
{
    public class AuthInput
    {
        public string client_id { get; set; }

        public string client_secret { get; set; }

        public string grant_type { get; set; }

        public string scope { get; set; }
    }
}