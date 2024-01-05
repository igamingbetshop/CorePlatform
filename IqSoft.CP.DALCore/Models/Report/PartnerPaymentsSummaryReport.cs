using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Report
{
    public class PartnerPaymentsSummaryReport
    {
        public List<PaymentMethod> PaymentMethods { get; set; }

        public List<PaymentInfo> PaymentsInfo { get; set; }
    }

    public class PaymentMethod
    {
        public int PaymentSystemId { get; set; }

        public string CurrencyId { get; set; }
    }

    public class PaymentInfo
    {
        public int ClientId {get; set;}

        public string FirstName {get; set;}

        public string LastName {get; set;}

        public string CurrencyId {get; set;}

        public List<PaymentMethodElement> Payments { get; set; }
    }

    public class PaymentMethodElement
    {
        public int PaymentSystemId { get; set; }

        public decimal Amount { get; set; }
    }
}
