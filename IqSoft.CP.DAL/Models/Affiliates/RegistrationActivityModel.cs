namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class RegistrationActivityModel
    {
        public int BrandId { get; set; }
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string BTag { get; set; }
        public string CountryCode { get; set; }
        public string RegistrationDate { get; set; }
        public string LanguageId { get; set; }
        public string RegistrationIp { get; set; }
        public string CustomerSourceID { get; set; }
    }
}