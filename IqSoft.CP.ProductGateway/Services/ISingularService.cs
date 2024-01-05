using System.ServiceModel;
using IqSoft.CP.ProductGateway.Models.Singular;

namespace IqSoft.CP.ProductGateway.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISingularService" in both code and config file together.
    [ServiceContract(Namespace = "https://productgateway.iqsoftllc.com/Services/Singular")]
    public interface ISingularService
    {
        [OperationContract(Name = "authenticateUserByToken")]
        AuthenticationOutput AuthenticateUserByToken(string operatorId, string token, string hash);

        [OperationContract(Name = "getBalance")]
        AmountResponse GetBalance(string operatorId, long userId, short currencyCode, bool isSingle, string hash);

        [OperationContract(Name = "depositMoney")]
        GenericOutput DepositMoney(string operatorId, long userId, short currencyCode, decimal amount, 
            bool isCardVerification, bool shouldWaitForApproval, string providerUserId, int? providerServiceId, 
            string transactionId, string additionalData, string requestorIp, string statusNote, string hash);

        [OperationContract(Name = "withdrawMoney")]
        WithdrawOutput WithdrawMoney(string operatorId, long userId, short currencyCode, decimal amount, bool shouldWaitForApproval, 
            string providerUserId, int? providerServiceId, string transactionId, string additionalData, string providerStatusCode, 
            string statusNote, string hash);

        [OperationContract(Name = "checkTransactionStatus")]
        GenericOutput CheckTransactionStatus(string operatorId, string transactionId, bool isCoreTransactionId, string hash);

        [OperationContract(Name = "rollbackTransaction")]
        int RollbackTransaction(string operatorId, string transactionOfProviderId, string transactionId, bool isCoreTransactionId,
            string statusNote, string hash);

        [OperationContract(Name = "getExchangeRates")]
        ExchangeRateOutput[] GetExchangeRates(string operatorId, string hash);

        [OperationContract(Name = "exchange")]
        AmountResponse Exchange(string operatorId, int sourceCurrencyId, int destinationCurrencyId, decimal amount,
            bool isReverse, string hash);
    }
}