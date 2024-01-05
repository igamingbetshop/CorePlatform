using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Qiwi
{
    [XmlType("response")]
    public class OutputBase
    {
        [XmlElement("osmp_txn_id")]
        public string OsmpTxnId { get; set; }

        [XmlElement("prv_txn")]
        public string Prvtxn { get; set; }

        [XmlElement("sum")]
        public decimal Sum { get; set; }

        [XmlElement("result")]
        public int Result { get; set; }

        [XmlElement("fields")]
        public Fields[] FieldArray { get; set; }
       
        [XmlElement("comment")]
        public string Comment { get; set; }
    }

    public class Fields
    {
        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}