using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.DPOPay
{
    [System.Xml.Serialization.XmlRootAttribute("API3G", Namespace = "", IsNullable = false)]
    public class PaymentInput
    {
        public string CompanyToken { get; set; }
        public string Request { get; set; }
        public TransactionInput Transaction { get; set; }
        public List<Service> Services { get; set; }
    }

    public class TransactionInput
    {
        public decimal PaymentAmount { get; set; }
        public string PaymentCurrency { get; set; }           
        public string CompanyRef { get; set; }           
        public string RedirectURL { get; set; }           
        public string BackURL { get; set; }           
        public int CompanyRefUnique { get; set; }      
    }

    public class Service
    {
        public string ServiceType { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceDate { get; set; }
    }
}
