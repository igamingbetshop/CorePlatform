namespace IqSoft.CP.ProductGateway.Models.PlaynGo.Output
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("authenticate", Namespace = "", IsNullable = false)]
    public class Authenticate
    {
        public string externalId { get; set; }
        public int statusCode { get; set; } = 0;
        public string statusMessage { get; set; } = "ok";
        public string userCurrency { get; set; }
        public string nickname { get; set; }
        public string country { get; set; }
        public string birthdate { get; set; }
        public string registration { get; set; }
        public string language { get; set; }
        public string affiliateId { get; set; }
        public decimal real { get; set; }
        public string externalGameSessionId { get; set; }

    }
}