using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
   public  class ApproveRequestInput : BaseInput
    {
        [XmlElement("pg_payment_id")]
        public string pg_payment_id { get; set; }

        [XmlElement("pg_approval_code")]
        public string pg_approval_code { get; set; }
    }
}
