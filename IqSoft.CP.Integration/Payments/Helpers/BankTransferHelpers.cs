using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class BankTransferHelpers
    {
        public static void PayPayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var notificationBl = new NotificationBll(clientBl))
                    {
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                           paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.PayPanding, string.Empty,
                                                            null, null, true, paymentRequest.Parameters, documentBl, notificationBl, false);
                    }
                }
            }
        }

        public static void ApprovePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw && paymentRequest.Status == (int)PaymentRequestStates.PayPanding)
            {
                using (var clientBl = new ClientBll(session, log))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {

                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                                               string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                    }
                }
            }
        }

        public static void PayPaymentRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit && paymentRequest.Status == (int)PaymentRequestStates.PayPanding)
            {

                using (var clientBl = new ClientBll(session, log))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        paymentRequest.ExternalTransactionId = paymentRequest.Id.ToString();
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    }
                }
            }
        }
    }
}