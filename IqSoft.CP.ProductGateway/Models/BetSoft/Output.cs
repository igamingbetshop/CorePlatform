using System.Xml.Serialization;
using IqSoft.CP.ProductGateway.Helpers;

namespace IqSoft.CP.ProductGateway.Models.BetSoft
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", ElementName = "EXTSYSTEM", IsNullable = false)]
    public class Output
    {
        [XmlElement(ElementName = "REQUEST")]
        public ExtSystemRequest Request { get; set; }

        [XmlElement()]
        public string TIME { get; set; }

        [XmlElement(ElementName = "RESPONSE")]
        public ExtSystemResponse Response { get; set; }
    }

    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRoot(Namespace = "", ElementName = "EXTSYSTEMREQUEST", IsNullable = false)]
    public class ExtSystemRequest
    {
        [XmlElement()]
        public string TOKEN { get; set; }

        [XmlElement()]
        public string HASH { get; set; }

        [XmlElement()]
        public string CLIENTTYPE { get; set; }

        [XmlElement()]
        public string USERID { get; set; }

        [XmlElement()]
        public string BET { get; set; }

        [XmlElement()]
        public string WIN { get; set; }

        [XmlElement()]
        public string BONUSID { get; set; }

        [XmlElement()]
        public string ROUNDID { get; set; }

        [XmlElement()]
        public string GAMEID { get; set; }

        [XmlElement()]
        public string ISROUNDFINISHED { get; set; }

        [XmlElement()]
        public string GAMESESSIONID { get; set; }

        [XmlElement()]
        public string NEGATIVEBET { get; set; }

        [XmlElement()]
        public string TRANSACTIONID { get; set; }

        [XmlElement()]
        public string CASINOTRANSACTIONID { get; set; }
    }

    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRoot(Namespace = "", ElementName = "EXTSYSTEMRESPONSE", IsNullable = false)]
    public class ExtSystemResponse
    {
        public ExtSystemResponse()
        {
            RESULT = BetSoftHelpers.ResponseResults.SuccessResponse;
        }
        [XmlElement()]
        public string RESULT { get; set; }

        [XmlElement()]
        public string CODE { get; set; }

        [XmlElement()]
        public string USERID { get; set; }

        [XmlElement()]
        public string USERNAME { get; set; }

        [XmlElement()]
        public string FIRSTNAME { get; set; }

        [XmlElement()]
        public string LASTNAME { get; set; }

        [XmlElement()]
        public string EMAIL { get; set; }

        [XmlElement()]
        public string CURRENCY { get; set; }

        [XmlElement()]
        public string BALANCE { get; set; }

        [XmlElement()]
        public string EXTSYSTEMTRANSACTIONID { get; set; }

        [XmlElement()]
        public string BONUSBET { get; set; }
    }
}