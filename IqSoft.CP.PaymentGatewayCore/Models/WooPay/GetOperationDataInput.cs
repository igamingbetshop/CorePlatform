using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.WooPay
{
    public class GetOperationDataInput
    {
        [XmlElement("operationId")]
        public int[] OperationId { get; set; }
    }
}