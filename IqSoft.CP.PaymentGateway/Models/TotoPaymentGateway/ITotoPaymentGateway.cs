using System.CodeDom.Compiler;
using System.ServiceModel;
using IqSoft.NGGP.WebApplications.PaymentGateway.Helpers;
using TotoGaming.Casino.Api.Logic.PaymentGateway;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.TotoPaymentGateway
{
    [GeneratedCode("System.ServiceModel", "4.0.0.0")]
    [ServiceContractAttribute(ConfigurationName = "ITotoPaymentGateway")]
    public interface ITotoPaymentGateway
    {
        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/CheckUser", ReplyAction = "http://tempuri.org/IServiceProxy/CheckUserResponse")]
        PaymentResult CheckUser(TotoPaymentGatewayHelpers.PaymentSystem ps, string userId);

        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/DepositConfirm", ReplyAction = "http://tempuri.org/IServiceProxy/DepositConfirmResponse")]
        PaymentResult DepositConfirm(TotoPaymentGatewayHelpers.PaymentSystem ps, int depositId, ReceiptInfo details);

        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/DepositFail", ReplyAction = "http://tempuri.org/IServiceProxy/DepositFailResponse")]
        PaymentResult DepositFail(TotoPaymentGatewayHelpers.PaymentSystem ps, int depositId, string failureMessage);
        
        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/DepositPrecheck", ReplyAction = "http://tempuri.org/IServiceProxy/DepositPrecheckResponse")]
        PaymentResult DepositPrecheck(TotoPaymentGatewayHelpers.PaymentSystem ps, int depositId, Amount amount);

        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/ReceiptConfirm", ReplyAction = "http://tempuri.org/IServiceProxy/ReceiptConfirmResponse")]
        PaymentResult ReceiptConfirm(TotoPaymentGatewayHelpers.PaymentSystem ps, string userId, ReceiptInfo details);

        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/GetPaymentRequest", ReplyAction = "http://tempuri.org/IServiceProxy/GetPaymentRequestResponse")]
        PaymentResult GetPaymentRequest(TotoPaymentGatewayHelpers.PaymentSystem ps, string receiptId, int transactionType);

        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/UpdatePaymentRequest", ReplyAction = "http://tempuri.org/IServiceProxy/UpdatePaymentRequestResponse")]
        PaymentResult UpdatePaymentRequest(PaymentRequestInfo requestInfo);


        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/ReceiptCancel", ReplyAction = "http://tempuri.org/IServiceProxy/ReceiptCancelResponse")]
        PaymentResult ReceiptCancel(TotoPaymentGatewayHelpers.PaymentSystem ps, string receiptId);

        [OperationContractAttribute(Action = "http://tempuri.org/IServiceProxy/DepositInitiate", ReplyAction = "http://tempuri.org/IServiceProxy/DepositInitiateResponse")]
        PaymentResult DepositInitiate(TotoPaymentGatewayHelpers.PaymentSystem ps, string userId, Amount amount);
    }
}
