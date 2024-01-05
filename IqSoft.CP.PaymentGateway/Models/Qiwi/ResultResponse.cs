using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Qiwi
{
    [XmlType("result")]
    public class ResultResponse
    {
        [XmlAttribute("result_code")]
        public int result_code { get; set; }
    }
}