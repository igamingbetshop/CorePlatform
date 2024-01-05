namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class InputBase
    {
        public int PartnerId { get; set; }
        public string CheckSum { get; set; }
        public string SecretKey { get; set; }
        public string LanguageId { get; set; }
    }
}
