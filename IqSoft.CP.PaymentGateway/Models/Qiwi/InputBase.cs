using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Qiwi
{
    public class InputBase
    {
        [XmlAttribute("command")]
        public string Command { get; set; }

        [XmlAttribute("txn_id")]
        public string Txn_id { get; set; }

        [XmlAttribute("account")]
        public string Account { get; set; }

        [XmlAttribute("sum")]
        public decimal Sum { get; set; }

        [XmlAttribute("pay_type")]
        public int Pay_Type { get; set; }

        [XmlAttribute("trm_id")]
        public long Trm_id { get; set; }

        [XmlAttribute("data1")]
        public string Data1 { get; set; }

        [XmlAttribute("billno")]
        public string BillNo { get; set; }

        [XmlAttribute("comsum")]
        public decimal Commission { get; set; }
    }
}