using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.MicroGaming
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", ElementName = "pkt", IsNullable = false)]
    public class Output
    {
        public Output()
        {
            MethodResponse = new Response();
        }

        [XmlElement(ElementName = "methodresponse")]
        public Response MethodResponse { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class Response
    {
        public Response()
        {
            Result = new ResponseResult();
        }

        [XmlElement(ElementName = "result")]
        public ResponseResult Result { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ResponseResult
    {
        public ResponseResult()
        {
            ExtInfo = new ExtInfo();
        }

        [XmlElement(ElementName = "extinfo")]
        public ExtInfo ExtInfo { get; set; }

        [XmlAttribute(AttributeName = "seq")]
        public string Seq { get; set; }

        [XmlAttribute(AttributeName = "token")]
        public string Token { get; set; }

        [XmlAttribute(AttributeName = "loginname")]
        public string LoginName { get; set; }

        [XmlAttribute(AttributeName = "currency")]
        public string Currency { get; set; }

        [XmlAttribute(AttributeName = "country")]
        public string Country { get; set; }

        [XmlAttribute(AttributeName = "city")]
        public string City { get; set; }

        [XmlAttribute(AttributeName = "balance")]
        public string Balance
        {
            get
            {
                if(AvailableBalance.HasValue)
                    return AvailableBalance.ToString();
                return null;
            }
        }

        [XmlIgnore]
        public int? AvailableBalance { get; set; }

        [XmlAttribute(AttributeName = "bonusbalance")]
        public string BonusBalance { get; set; }

        [XmlAttribute(AttributeName = "wallet")]
        public string Wallet { get; set; }

        [XmlAttribute(AttributeName = "exttransactionid")]
        public string ExtTransactionId { get; set; }

        [XmlAttribute(AttributeName = "errorcode")]
        public string ErrorCode { get; set; }

        [XmlAttribute(AttributeName = "errordescription")]
        public string ErrorDescription { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ExtInfo
    {
    }
}