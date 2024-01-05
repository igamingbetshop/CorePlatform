using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.WooPay
{
    public class CreateInvoiceOutput : BaseOutput
    {
        [XmlElement("response")]
        public CreateInvoiceOutputData Response { get; set; }
    }

    public class CreateInvoiceOutputData
    {
        [XmlElement("operationId")]
        public long OperationId { get; set; }

        [XmlElement("operationUrl")]
        public string OperationUrl { get; set; }
    }
}