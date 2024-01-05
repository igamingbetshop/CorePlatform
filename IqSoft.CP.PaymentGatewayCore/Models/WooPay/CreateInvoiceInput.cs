using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.WooPay
{
    public class CreateInvoiceInput
    {
        [XmlElement("referenceId")]
        public string ReferenceId { get; set; }

        [XmlElement("backUrl")]
        public string BackUrl { get; set; }

        [XmlElement("requestUrl")]
        public string RequestUrl { get; set; }

        [XmlElement("addInfo")]
        public string AddInfo { get; set; }

        [XmlElement("amount")]
        public float Amount { get; set; }

        [XmlElement("deathDate")]
        public string DeathDate { get; set; }

        [XmlElement("serviceType")]
        public int ServiceType { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("orderNumber")]
        public int OrderNumber { get; set; }

        [XmlElement("userEmail")]
        public string UserEmail { get; set; }

        [XmlElement("userPhone")]
        public string UserPhone { get; set; }
    }
}