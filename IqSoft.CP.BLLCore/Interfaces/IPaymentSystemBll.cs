using System.Collections.Generic;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Models.Report;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IPaymentSystemBll : IBaseBll
    {
        List<PaymentSystem> GetPaymentSystems();

        PaymentSystem SavePaymentSystem(PaymentSystem paymentSystem);
        
        List<fnPartnerPaymentSetting> GetfnPartnerPaymentSettings(FilterfnPartnerPaymentSetting filter, bool checkPermissions);

        PartnerPaymentSetting SavePartnerPaymentSetting(PartnerPaymentSetting partnerPaymentSetting);

        List<PaymentRequestHistoryElement> GetPaymentRequestHistories(List<long> requestId, int? status = null);

        List<fnPaymentRequest> GetPaymentRequests(FilterfnPaymentRequest filter, bool checkPermissions);

        PaymentRequestsReport GetPaymentRequestsPaging(FilterfnPaymentRequest filter, bool convertCurrency, bool checkPermissions);

        PaymentRequest GetPaymentRequestById(long id);

        List<fnPaymentRequest> ExportDepositPaymentRequests(FilterfnPaymentRequest filter);

        List<fnPaymentRequest> ExportWithdrawalPaymentRequests(FilterfnPaymentRequest filter);
		
		List<PaymentRequestHistory> GetPaymentRequestComments(long requestId);
	}
}
