namespace IqSoft.CP.ProductGateway.Models.PlaynGo.Output
{
    [System.Xml.Serialization.XmlRootAttribute("balance", Namespace = "", IsNullable = false)]
    public class Balance
    {
        public string externalTransactionId { get; set; }
        public decimal real { get; set; }
        public int statusCode { get; set; } = 0;
        public string statusMessage { get; set; } 
    }

    [System.Xml.Serialization.XmlRootAttribute("reserve", Namespace = "", IsNullable = false)]
    public class Reserve : Balance { }

    [System.Xml.Serialization.XmlRootAttribute("release", Namespace = "", IsNullable = false)]
    public class Release : Balance { }

    [System.Xml.Serialization.XmlRootAttribute("cancelReserve", Namespace = "", IsNullable = false)]
    public class CancelReserve : Balance { }
}