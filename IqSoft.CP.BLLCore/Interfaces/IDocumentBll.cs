﻿using System.Collections.Generic;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Documents;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IDocumentBll : IBaseBll
    {
        Document CreateDocument(Operation operation);
        Document DeleteDocument(Document inputDocument);
        //if ignoreLowBalance = true withdraw all amount from this object
        List<Transaction> CreateCreditTransaction(OperationItem operationItem, bool isBonusTransaction, bool freezeBonusBalance, bool ignoreLowBalance, out bool fromBonusBalance);
        Account GetOrCreateAccount(int objectId, int objectTypeId, string currency, int accountType, bool clearCach);
        List<Document> GetDocuments(FilterDocument filter);
        Document GetDocumentById(long id, string include = "");
		List<Document> GetDocumentsByParentId(long id);
		Note SaveNote(Note note);
        PagedModel<Transaction> GetTransactions(FilterTransaction filter);
        PagedModel<fnTransaction> GetFnTransactions(FilterFnTransaction filter);
        List<Document> RollbackProductTransactions(ListOfOperationsFromApi transactions, bool checkProduct = true, string newExternalId = null);
        List<ClientBonu> FinalizeWageringBonusDocument();
        Document CreateBonusDocumnet(Client client, decimal bonusPrice, int bonusOperationType, int accountTypeId);
        List<Document> RollBackPaymentRequest(List<Document> existingDocuments);
        Document GetDocumentOnlyByExternalId(string externalTransactionId, int gameProviderId, int clientId, int operationTypeId);
        Document GetDocumentByExternalId(string externalTransactionId, int clientId, int gameProviderId, int partnerProductSettingId, int operationTypeId);
        Document GetDocumentByRoundId(int operationTypeId, string roundId, int providerId, int clientId, int? state = null);
    }
}
