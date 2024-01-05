using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.WooPay
{
    public class GetOperationDataOutput : BaseOutput
    {
        [XmlElement("response")]
        public GetOperationDataOutputData[] Response { get; set; }
    }

    public class GetOperationDataOutputData
    {
        [XmlElement("id")]
        public long Id { get; set; }

        [XmlElement("type")]
        public int Type { get; set; }

        [XmlElement("lotId")]
        public int LotId { get; set; }

        [XmlElement("sum")]
        public int Sum { get; set; }

        [XmlElement("date")]
        public string Date { get; set; }

        [XmlElement("status")]
        public int Status { get; set; }

        [XmlElement("comment")]
        public string Comment { get; set; }

        [XmlElement("fromSubject")]
        public string FromSubject { get; set; }

        [XmlElement("toSubject")]
        public string ToSubject { get; set; }

        [XmlElement("fromFullName")]
        public string FromFullName { get; set; }

        [XmlElement("toFullName")]
        public string ToFullName { get; set; }
    }
}