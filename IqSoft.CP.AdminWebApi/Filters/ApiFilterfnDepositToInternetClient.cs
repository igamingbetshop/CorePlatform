using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterfnDepositToInternetClient : ApiFilterBase
    {
        public long? Id { get; set; }

        public string ExternalTransactionId { get; set; }

        public int? ParentId { get; set; }

        public long? PaymentRequestId { get; set; }

        public List<int> OperationTypeIds { get; set; }

        public int? PartnerPaymentSettingId { get; set; }

        public int? PaymentSystemId { get; set; }

        public int? BetShopId { get; set; }

        public string BetShopName { get; set; }

        public string Info { get; set; }

        public int? GameProviderId { get; set; }

        public int? PartnerProductId { get; set; }

        public int? ExternalOperationId { get; set; }

        public long? Barcode { get; set; }

        public string TicketInfo { get; set; }

        public int? UserId { get; set; }

        public int? ClientId { get; set; }

        public int? State { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}