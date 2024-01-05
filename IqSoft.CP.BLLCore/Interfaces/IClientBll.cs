using System;
using System.Collections.Generic;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Filters.Clients;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Job;
using IqSoft.CP.DAL;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models.Payments;
using IqSoft.CP.Common.Models;
using IqSoft.CP.BLL.Services;
using Microsoft.AspNetCore.Hosting;
using IqSoft.CP.Common.Models.WebSiteModels;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IClientBll : IBaseBll
    {
        Client RegisterClient(ClientRegistrationInput clientRegistrationInput, IWebHostEnvironment env);

        List<int> VerifyClientMobileNumber(string key, string mobileNumber, int? clientId, int partnerId, bool expire,
                                           List<Common.Models.WebSiteModels.SecurityQuestion> securityQuestions, bool checkSecQuestions = true);

        List<int> VerifyClientEmail(string key, string mobileNumber, int? clientId, int partnerId, bool expire,
                                    List<Common.Models.WebSiteModels.SecurityQuestion> securityQuestions, bool checkSecQuestions = true);

        Client GetClientById(int id, bool checkPermission = false);

        List<Client> GetClients(FilterClient filter);

        PagedModel<fnClient> GetfnClientsPagedModel(FilterfnClient filter, bool checkPermission);

        BllClient ProductsAuthorization(string token, out string newToken, out int productId, out string languageId, bool expireOld = false);

        BllClient GetClientByToken(string token, out ClientLoginOut clientLoginOut, string languageId);

        ClientSession RefreshClientSession(string token, bool expireOld = false);

        int LogoutClient(string token);

        void LogoutClientById(int clientId, int? logoutType = null);

        void ChangeClientPassword(ChangeClientPasswordInput input, bool isReset = false);

        void ChangeClientPassword(NewPasswordInput input);

        Client ChangeClientState(int clientId, int? state, bool? isDocumentVerified);

        PagedModel<ClientSession> GetClientLoginsPagedModel(FilterClientSession filter);

        int SendRecoveryToken(int partnerId, string languageId, string identifier, string recaptcha);

        Client RecoverPassword(int partnerId, string recoveryToken, string newPassword,
            string languageId, List<Common.Models.WebSiteModels.SecurityQuestion> securityQuestions);

        DAL.Models.Clients.ClientInfo GetClientInfo(int clientId, bool checkPermission);

        Note SaveNote(Note note);

        List<AccountsBalanceHistoryElement> GetClientAccountsBalanceHistoryPaging(FilterAccountsBalanceHistory filter);

        PagedModel<fnClientLog> GetClientLogs(FilterfnClientLog filter);

        List<fnClientIdentity> GetClientIdentityInfo(int clientId, bool checkPermission);

        ResponseBase SetPaymentLimit(PaymentLimit paymentLimit, bool checkPermission);

        PaymentLimit GetPaymentLimit(int clientId);
        List<fnAccountType> GetAccountTypes(string languageId);

        List<fnClient> ExportClients(FilterfnClient filter);

        List<fnClientIdentity> ExportClientIdentity(int clientId);

        List<ClientProductBet> GetCashBackBonusBets(int partnerId, DateTime fromDate, DateTime toDate);

        List<ClientCategory> GetClientCategories();

        PaymentLimit GetPaymentLimitExclusion(int clientId, bool checkPermission);

        ResponseBase SetPaymentLimitExclusion(PaymentLimit paymentLimit);

        ClientPaymentInfo RegisterClientPaymentAccountDetails(ClientPaymentInfo info, string code, bool checkCode);

        List<ClientPaymentInfo> GetClientPaymentAccountDetails(int clientId, int? paymentSystemId, List<int> accountTypes, bool checkPermission);

        List<AffiliateClientModel> GetClientsOfAffiliateManager(int managerId, int hours);

        List<fnTicket> OpenTickets(Ticket ticket, TicketMessage message, List<int> clientIds, bool checkPermissions);

        #region ClientDocument
        PaymentRequest CreateWithdrawPaymentRequest(PaymentRequestModel request, decimal percent, BllClient client, DocumentBll documentBl, NotificationBll notificationBl);

        ChangeWithdrawRequestStateOutput ChangeWithdrawRequestState(long requestId, PaymentRequestStates state, string comment,
            int? cashDeskId, int? cashierId, bool checkPermission, string parameters, DocumentBll documentBl, NotificationBll notificationBl, bool sendEmail);

        PaymentRequest CreateDepositFromPaymentSystem(PaymentRequest request);
        // Transfer money from PartnerPaymentSetting to client
        Document ApproveDepositFromPaymentSystem(PaymentRequest request, bool fromAdmin, string comment = "", ClientPaymentInfo info = null, MerchantRequest mr = null);

        // Transfer money from BetShop to client
        PaymentRequest CreateDepositFromBetShop(PaymentRequest transaction);

        // Approve deposit payment request from BetShop
        void ApproveDepositFromBetShop(IPaymentSystemBll paymentSystemBl, long requestId, string comment, DocumentBll documentBl, NotificationBll notificationBl);

        // Reject deposit payment request from BetShop
        void CancelDeposit(long requestId, string comment);

        void ChangeDepositRequestState(long requestId, PaymentRequestStates state, string comment, NotificationBll notificationBll, bool checkPermission = false);

        Document PayWithdrawFromPaymentSystem(ChangeWithdrawRequestStateOutput resp, DocumentBll documentBl, NotificationBll notificationBl, MerchantRequest mr = null);

        // Debit Correction On CashDesk
        Document CreateDebitCorrectionOnClient(ClientCorrectionInput correction, DocumentBll documentBl, bool checkPermission);

        // Credit Correction On CashDesk
        Document CreateCreditCorrectionOnClient(ClientCorrectionInput correction, DocumentBll documentBl, bool checkPermission);

        Document CreateCreditFromClient(ListOfOperationsFromApi transaction, DocumentBll documentBl);

        List<Document> CreateDebitsToClients(ListOfOperationsFromApi transactions, Document creditTransaction, DocumentBll documentBl);

        Document CreateDebitToClient(ClientOperation transaction, int clientId, string userName, DocumentBll documentBl, Document creditTransaction);

        void ChangeWithdrawPaymentRequestState(long requestId, string comment, int? cashDeskId, int? cashierId, PaymentRequestStates state);

        PayWithdrawFromBetShopOutput PayWithdrawFromBetShop(ChangeWithdrawRequestStateOutput resp, int cashDeskId, int? cashierId, DocumentBll documentBl);

        #endregion
    }
}
