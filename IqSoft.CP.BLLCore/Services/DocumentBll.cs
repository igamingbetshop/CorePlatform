using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Documents;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Document;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace IqSoft.CP.BLL.Services
{
    public class DocumentBll : PermissionBll, IDocumentBll
    {
        #region Constructors

        public DocumentBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public DocumentBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public Document CreateDocument(Operation operation)
        {
            if (operation.Amount < 0)
                throw CreateException(LanguageId, Constants.Errors.WrongOperationAmount);
            var currentDate = GetServerDate();
            var date = (long)currentDate.Year * 1000000 + (long)currentDate.Month * 10000 + (long)currentDate.Day * 100 + (long)currentDate.Hour;

            var document = new Document
            {
                ExternalTransactionId = operation.ExternalTransactionId,
                Amount = operation.Amount,
                CurrencyId = operation.CurrencyId,
                ExternalOperationId = operation.ExternalOperationId,
                TicketNumber = operation.TicketNumber,
                TicketInfo = operation.TicketInfo,
                CashDeskId = operation.CashDeskId,
                PartnerPaymentSettingId = operation.PartnerPaymentSettingId,
                PaymentRequestId = operation.PaymentRequestId,
                PartnerProductId = operation.PartnerProductId,
                ProductId = operation.ProductId,
                GameProviderId = operation.GameProviderId,
                SessionId = Identity.IsAdminUser ? Identity.SessionId : operation.SessionId,
                ClientId = operation.ClientId,
                Info = operation.Info,
                ParentId = operation.ParentId,
                OperationTypeId = operation.Type,
                UserId = operation.UserId,
                Creator = operation.Creator,
                DeviceTypeId = operation.DeviceTypeId,
                TypeId = operation.DocumentTypeId,
                State = operation.State ?? (int)BetDocumentStates.Uncalculated,
                RoundId = operation.RoundId,
                PossibleWin = operation.PossibleWin,
                CreationTime = currentDate,
                LastUpdateTime = currentDate,
                Date = date
            };

            var debitTrans =
                operation.OperationItems.Where(x => x.Type == (int)TransactionTypes.Debit).ToList();
            var creditTrans =
                operation.OperationItems.Where(x => x.Type == (int)TransactionTypes.Credit).ToList();
            document.Transactions = new List<Transaction>();
            foreach (var operationItem in debitTrans)
            {
                document.Transactions.Add(CreateDebitTransaction(operationItem));
            }

            bool isFromBonusBalance = false;
            foreach (var operationItem in creditTrans)
            {
                var transactions = CreateCreditTransaction(operationItem, (operation.BonusId.HasValue && operation.BonusId.Value > 0),
                    operation.FreezeBonusBalance ?? false, false, out bool fromBonusBalance);
                transactions.ForEach(x => document.Transactions.Add(x));
                if (fromBonusBalance && !isFromBonusBalance)
                    isFromBonusBalance = true;
            }
            if (isFromBonusBalance)
                document.Info = JsonConvert.SerializeObject(new DocumentInfo { BonusId = operation.BonusId ?? 0, FromBonusBalance = true });

            Db.Documents.Add(document);
            return document;
        }

        public Document DeleteDocument(Document inputDocument)
        {
            var doc =
                Db.Documents.Include(x => x.Transactions).ThenInclude(x=>x.Account).FirstOrDefault(x => x.Id == inputDocument.Id);
            if (doc == null)
                throw CreateException(LanguageId, Constants.Errors.DocumentNotFound);
            if (doc.State == (int)BetDocumentStates.Paid)
                throw CreateException(LanguageId, Constants.Errors.CantDeleteDocumentFromThisState);

            int operationTypeId = (int)OperationTypes.Rollback;
            int bonusId = 0;
            if (doc.OperationTypeId == (int)OperationTypes.Bet)
            {
                operationTypeId = (int)OperationTypes.BetRollback;
                Int32.TryParse(doc.Info, out bonusId);
            }
            else if (doc.OperationTypeId == (int)OperationTypes.Win)
                operationTypeId = (int)OperationTypes.WinRollback;

            if (doc.State == (int)BetDocumentStates.Deleted)
                return Db.Documents.FirstOrDefault(x => x.ParentId == doc.Id && x.OperationTypeId == operationTypeId);

            if (bonusId > 0 && !string.IsNullOrEmpty(doc.TicketInfo))
                RecoverClientFreeBetBonus(bonusId, doc.ClientId.Value, doc.TicketInfo);

            doc.State = (int)BetDocumentStates.Deleted;
            var currentDate = GetServerDate();
            doc.LastUpdateTime = currentDate;

            if (doc.OperationTypeId == (int)OperationTypes.DepositRollback)
            {
                operationTypeId = (int)OperationTypes.DepositRollback;
            }
            else if (doc.OperationTypeId == (int)OperationTypes.WithdrawRollback)
            {
                operationTypeId = (int)OperationTypes.WithdrawRollback;
            }

            var document = new Document
            {
                ExternalTransactionId = doc.ExternalTransactionId,
                SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                Amount = doc.Amount,
                CurrencyId = doc.CurrencyId,
                State = (int)BetDocumentStates.Deleted,
                ParentId = inputDocument.Id,
                PaymentRequestId = doc.PaymentRequestId,
                UserId = doc.UserId,
                CashDeskId = doc.CashDeskId,
                PartnerPaymentSettingId = doc.PartnerPaymentSettingId,
                PartnerProductId = doc.PartnerProductId,
                GameProviderId = doc.GameProviderId,
                ClientId = doc.ClientId,
                CreationTime = currentDate,
                LastUpdateTime = currentDate,
                OperationTypeId = operationTypeId
            };
            Db.Documents.Add(document);
            var creditTrans = doc.Transactions.Where(x => x.Type == (int)TransactionTypes.Credit).ToList();
            var transactions = creditTrans.Select(transaction => new OperationItem
            {
                AccountId = transaction.AccountId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = operationTypeId,
                CurrencyId = transaction.Account.CurrencyId,
                Amount = transaction.Amount,
                AccountTypeId = transaction.Account.TypeId,
                ObjectTypeId = transaction.Account.ObjectTypeId
            }).Select(CreateDebitTransaction).ToList();
            var debitTrans = doc.Transactions.Where(x => x.Type == (int)TransactionTypes.Debit).ToList();
            foreach (var transaction in debitTrans)
            {
                var operationItem = new OperationItem
                {
                    AccountId = transaction.AccountId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = operationTypeId,
                    CurrencyId = transaction.Account.CurrencyId,
                    Amount = transaction.Amount
                };
                transactions.AddRange(CreateCreditTransaction(operationItem, false, false, true, out bool fromBonusBalance));
            }
            document.Transactions = transactions;
            Db.SaveChanges();
            var chaildDocuments = Db.Documents.Where(x => x.ParentId == inputDocument.Id && x.Id != document.Id).ToList();
            foreach (var d in chaildDocuments)
            {
                DeleteDocument(d);
            }
            return document;
        }

        private void RecoverClientFreeBetBonus(int bonusId, int clientId, string ticketInfo)
        {
            var bonus = Db.ClientBonus.Include(x => x.Bonus).FirstOrDefault(x => x.BonusId == bonusId && x.ClientId == clientId);
            if (bonus == null || bonus.Bonus.RefundRollbacked == null || !bonus.Bonus.RefundRollbacked.Value)
                return;

            var info = JsonConvert.DeserializeObject<TicketInfo>(ticketInfo);

            bonus.TurnoverAmountLeft += info.Amount;
            bonus.Status = (int)BonusStatuses.Active;
            Db.SaveChanges();
        }

        private Transaction CreateDebitTransaction(OperationItem operationItem)
        {
            bool willBalanceChange = (operationItem.ObjectTypeId != (int)ObjectTypes.Partner && operationItem.ObjectTypeId != (int)ObjectTypes.PartnerProduct);
            Account account = null;
            if (operationItem.AccountId.HasValue)
            {
                if (willBalanceChange)
                {
                    Db.Procedures.sp_GetAccountLockByIdAsync(operationItem.AccountId.Value).Wait();
                    account = GetAccount(operationItem.AccountId.Value);
                }
                else
                {
                    account = new Account { Id = operationItem.AccountId.Value };
                }
            }
            else
            {
                if (!operationItem.AccountTypeId.HasValue)
                    throw CreateException(LanguageId, Constants.Errors.AccountNotFound);

                var accountId = CreateAccountIfNotExists(operationItem.ObjectId, operationItem.ObjectTypeId,
                    operationItem.CurrencyId, operationItem.AccountTypeId.Value);

                if (willBalanceChange)
                {
                    Db.Procedures.sp_GetAccountLockByIdAsync(accountId).Wait();
                    account = GetAccount(accountId);
                }
                else
                {
                    account = new Account { Id = accountId };
                }
            }
            var currentDate = GetServerDate();
            var transaction = new Transaction
            {
                AccountId = account.Id,
                Amount = operationItem.Amount,
                OperationTypeId = operationItem.OperationTypeId,
                Type = operationItem.Type,
                CreationTime = currentDate,
                Date = currentDate.Year * 1000000 + currentDate.Month * 10000 + currentDate.Day * 100 + currentDate.Hour
            };
            if (operationItem.Amount > 0)
            {
                if (willBalanceChange)
                {
                    Db.Entry(account).Reload();
                    ChangeAccountBalance(operationItem.Amount, account);
                }
            }
            return transaction;
        }

        //if ignoreLowBalance = true withdraw all amount from this object (credit == withdraw)
        public List<Transaction> CreateCreditTransaction(OperationItem operationItem, bool isBonusTransaction, 
            bool freezeBonusBalance, bool ignoreLowBalance, out bool fromBonusBalance)
        {
            var currentTime = GetServerDate();
            var accounts = new List<Account>();
            var transactions = new List<Transaction>();
            decimal amount;
            fromBonusBalance = false;

            bool willBalanceChange = (operationItem.ObjectTypeId != (int)ObjectTypes.Partner && operationItem.ObjectTypeId != (int)ObjectTypes.PartnerProduct);

            if (operationItem.AccountId.HasValue || operationItem.AccountTypeId.HasValue)
            {
                if (willBalanceChange)
                {
                    Account account = null;
                    if (operationItem.AccountId.HasValue)
                    {
                        Db.Procedures.sp_GetAccountLockByIdAsync(operationItem.AccountId).Wait();
                        account = GetAccount(operationItem.AccountId.Value);
                    }
                    else
                    {
                        Db.Procedures.sp_GetAccountLockAsync(operationItem.ObjectId,
                            operationItem.ObjectTypeId, operationItem.CurrencyId, operationItem.AccountTypeId).Wait();
                        account = GetOrCreateAccount(operationItem.ObjectId, operationItem.ObjectTypeId,
                            operationItem.CurrencyId, operationItem.AccountTypeId.Value);
                    }

                    var accountType = CacheManager.GetAccountTypeById(account.TypeId);
                    if (account.Balance -
                        ConvertCurrency(operationItem.CurrencyId, account.CurrencyId, operationItem.Amount) < 0 &&
                        (!accountType.CanBeNegative && !ignoreLowBalance))
                        throw CreateException(LanguageId, Constants.Errors.LowBalance);
                    accounts.Add(account);
                }
                else
                {
                    long accountId = 0;
                    if (operationItem.AccountId.HasValue)
                        accountId = operationItem.AccountId.Value;
                    else
                    {
                        var account = GetOrCreateAccount(operationItem.ObjectId, operationItem.ObjectTypeId,
                            operationItem.CurrencyId, operationItem.AccountTypeId.Value);
                        accountId = account.Id;
                    }

                    var transaction = new Transaction
                    {
                        AccountId = accountId,
                        Amount = operationItem.Amount,
                        OperationTypeId = operationItem.OperationTypeId,
                        Type = operationItem.Type,
                        CreationTime = currentTime,
                        Date = currentTime.Year * 1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour
                    };
                    transactions.Add(transaction);
                }
            }
            else
            {
                amount = operationItem.Amount;
                var orderedAccountTypes = CacheManager.GetOrderedAccountTypesByOperationTypeId(operationItem.OperationTypeId);
                if (!isBonusTransaction || freezeBonusBalance)
                    orderedAccountTypes = orderedAccountTypes.Where(x => x != (int)AccountTypes.ClientBonusBalance).ToList();

                var accountsInfo = CacheManager.GetAccountsInfo(operationItem.ObjectTypeId, operationItem.ObjectId,
                    operationItem.CurrencyId);
                var accountsWithSpecificCurrency = (from t in orderedAccountTypes
                                                    select accountsInfo.FirstOrDefault(x => x.TypeId == t)
                    into info
                                                    where info != null
                                                    select info.Id).ToList();

                if (willBalanceChange)
                {
                    foreach (var accountId in accountsWithSpecificCurrency)
                    {
                        Db.Procedures.sp_GetAccountLockByIdAsync(accountId).Wait();
                        var acc = GetAccount(accountId);
                        if (operationItem.Amount <= 0 || acc.Balance > 0)
                        {
                            accounts.Add(acc);
                            amount -= acc.Balance;
                            if (amount <= 0 && operationItem.Amount > 0)
                                break;
                        }
                    }

                    if (amount > 0 && !ignoreLowBalance)
                        throw CreateException(LanguageId, Constants.Errors.LowBalance);
                }
                else
                {
                    var transaction = new Transaction
                    {
                        AccountId = accountsWithSpecificCurrency.Any()
                            ? accountsWithSpecificCurrency[0]
                            : GetOrCreateAccount(operationItem.ObjectId, operationItem.ObjectTypeId,
                                operationItem.CurrencyId, operationItem.AccountTypeId.Value).Id,
                        Amount = operationItem.Amount,
                        OperationTypeId = operationItem.OperationTypeId,
                        Type = operationItem.Type,
                        CreationTime = currentTime,
                        Date = currentTime.Year * 1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour
                    };
                    transactions.Add(transaction);
                }
            }
            
            if (willBalanceChange)
            {
                amount = operationItem.Amount;
                var bonusBalance = accounts.Where(x => x.TypeId == (int)AccountTypes.ClientBonusBalance).Sum(x => x.Balance);
                if (bonusBalance > 0 && amount == 0)
                {
                    if (accounts.Where(x => x.TypeId == (int)AccountTypes.ClientUnusedBalance ||
                        x.TypeId == (int)AccountTypes.ClientUsedBalance ||
                        x.TypeId == (int)AccountTypes.AffiliateManagerBalance).Sum(x => x.Balance) == 0)
                        fromBonusBalance = true;
                }

                for (var i = 0; i < accounts.Count && amount > 0; i++)
                {
                    decimal transactionAmount;
                    decimal amountToCreditFromAccount;
                    var accountType = CacheManager.GetAccountTypeById(accounts[i].TypeId);
                    if (accountType.CanBeNegative)
                        amountToCreditFromAccount = amount;
                    else
                        amountToCreditFromAccount = ConvertCurrency(accounts[i].CurrencyId, operationItem.CurrencyId, accounts[i].Balance);

                    if (amountToCreditFromAccount < amount)
                    {
                        transactionAmount = accounts[i].Balance;
                        amount -= amountToCreditFromAccount;
                    }
                    else
                    {
                        transactionAmount = ConvertCurrency(operationItem.CurrencyId, accounts[i].CurrencyId, amount);
                        amount = 0;
                    }

                    var transaction = new Transaction
                    {
                        AccountId = accounts[i].Id,
                        Amount = transactionAmount,
                        OperationTypeId = operationItem.OperationTypeId,
                        Type = operationItem.Type,
                        CreationTime = currentTime,
                        Date = currentTime.Year * 1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour,
                        AccountTypeId = accounts[i].TypeId
                    };
                    transactions.Add(transaction);
                    ChangeAccountBalance(transactionAmount * -1, accounts[i]);
                    if (transactionAmount > 0 && accounts[i].TypeId == (int)AccountTypes.ClientBonusBalance)
                        fromBonusBalance = true;
                }
                if (isBonusTransaction && bonusBalance > 0 && operationItem.Amount == 0)
                {
                    var transaction = new Transaction
                    {
                        AccountId = accounts[0].Id,
                        Amount = 0,
                        OperationTypeId = operationItem.OperationTypeId,
                        Type = operationItem.Type,
                        CreationTime = currentTime,
                        Date = currentTime.Year * 1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour,
                        AccountTypeId = (int)AccountTypes.ClientBonusBalance
                    };
                    transactions.Add(transaction);
                }
            }
            return transactions;
        }

        public Account GetOrCreateAccount(int objectId, int objectTypeId, string currency, int accountType, bool clearCache = true)
        {
            var account = Db.Accounts.FirstOrDefault(x =>
                x.ObjectId == objectId && x.ObjectTypeId == objectTypeId && x.CurrencyId == currency &&
                x.TypeId == accountType);

            if (account != null)
            {
                if (Db.Entry(account).State != EntityState.Unchanged)
                    Db.Entry(account).Reload();
                return account;
            }
            var dbDate = GetServerDate();
            account = new Account
            {
                ObjectId = objectId,
                ObjectTypeId = objectTypeId,
                TypeId = accountType,
                Balance = 0,
                CurrencyId = currency,
                SessionId = SessionId,
                CreationTime = dbDate,
                LastUpdateTime = dbDate
            };
            Db.Accounts.Add(account);
            Db.SaveChanges();
            if (clearCache)
                CacheManager.RemoveAccountsInfo(objectTypeId, objectId, currency);
            return account;
        }

        public long CreateAccountIfNotExists(int objectId, int objectTypeId, string currency, int accountType, bool clearCache = true)
        {
            var accountId = Db.Accounts.Where(x => x.ObjectId == objectId && x.ObjectTypeId == objectTypeId && x.CurrencyId == currency &&
                        x.TypeId == accountType).Select(x => x.Id).FirstOrDefault();

            if (accountId > 0)
                return accountId;

            var dbDate = GetServerDate();
            var account = new Account
            {
                ObjectId = objectId,
                ObjectTypeId = objectTypeId,
                TypeId = accountType,
                Balance = 0,
                CurrencyId = currency,
                SessionId = SessionId,
                CreationTime = dbDate,
                LastUpdateTime = dbDate
            };
            Db.Accounts.Add(account);
            Db.SaveChanges();
            if (clearCache)
                CacheManager.RemoveAccountsInfo(objectTypeId, objectId, currency);
            return account.Id;
        }

        public Document GetDocumentById(long id, string include = "")
        {
            var query = Db.Documents.AsQueryable();
            if (!string.IsNullOrEmpty(include))
                query = query.Include(include);
            return query.FirstOrDefault(x => x.Id == id);
        }
        public List<Document> GetDocumentsByParentId(long id)
        {
            return Db.Documents.Where(x => x.ParentId == id).ToList();
        }

        public List<DAL.Document> GetDocuments(FilterDocument filter)
        {
            return filter.FilterObjects(Db.Documents).ToList();
        }

        public Document GetDocumentFromDb(int partnerPaymentSettingId, string externalTransactionId, int operationTypeId)
        {
            return Db.Documents.Where(x => x.ExternalTransactionId == externalTransactionId &&  x.OperationTypeId == operationTypeId &&
                                           x.PartnerPaymentSettingId == partnerPaymentSettingId).FirstOrDefault();
        }


        public long GetExistingDocumentId(int providerId, string transactionId, int operationTypeId, int? productId)
        {
            return Db.Documents.Where(x => x.ExternalTransactionId == transactionId && x.OperationTypeId == operationTypeId &&
            x.GameProviderId == providerId && x.ProductId == productId).Select(x => x.Id).FirstOrDefault();
        }

        public PagedModel<Transaction> GetTransactions(FilterTransaction filter)
        {
            var transactions = new PagedModel<Transaction>
            {
                Entities = filter.FilterObjects(Db.Transactions, transaction => transaction.OrderBy(x => x.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.Transactions)
            };
            return transactions;
        }

        public PagedModel<fnTransaction> GetFnTransactions(FilterFnTransaction filter)
        {
            var transactions = new PagedModel<fnTransaction>
            {
                Entities = filter.FilterObjects(Db.fn_Transaction(LanguageId), transaction => transaction.OrderByDescending(x => x.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_Transaction(LanguageId))
            };
            return transactions;
        }

        public Note SaveNote(Note note)
        {
            CheckPermission(Constants.Permissions.CreateNote);
            var document = GetDocumentById(Convert.ToInt64(note.ObjectId));
            if (document == null)
                throw CreateException(LanguageId, Constants.Errors.DocumentNotFound);
            var currentTime = GetServerDate();
            var dbNote = Db.Notes.FirstOrDefault(x => x.Id == note.Id);

            if (dbNote == null)
            {
                dbNote = new Note { CreationTime = currentTime, SessionId = SessionId };
                Db.Notes.Add(dbNote);
            }
            note.CreationTime = dbNote.CreationTime;
            note.SessionId = dbNote.SessionId;
            note.LastUpdateTime = currentTime;
            Db.Entry(dbNote).CurrentValues.SetValues(note);

            if ((document.HasNote == false || document.HasNote == null) && note.Type == (int)NoteTypes.Standard && note.State == (int)NoteStates.Active)
            {
                document.HasNote = true;
                document.LastUpdateTime = currentTime;
            }
            else if (document.HasNote == true && note.Type == (int)NoteTypes.Standard && note.State == (int)NoteStates.Deleted)
            {
                var notesCount = Db.Notes.Count(x => x.ObjectTypeId == (int)ObjectTypes.Document && x.ObjectId == document.Id &&
                    x.Id != note.Id && x.State == (int)NoteStates.Active);
                if (notesCount == 0)
                    document.HasNote = false;
            }
            SaveChanges();
            return dbNote;
        }

        public List<Document> RollbackProductTransactions(ListOfOperationsFromApi transactions, bool checkProduct = true, string newExternalId = null)
        {
            var documents = new List<Document>();

            var existingDocuments = new List<Document>();
            if (checkProduct)
            {
                var product = transactions.ProductId.HasValue
                    ? CacheManager.GetProductById(transactions.ProductId.Value)
                    : CacheManager.GetProductByExternalId(transactions.GameProviderId, transactions.ExternalProductId);
                if (product == null)
                    throw CreateException(LanguageId, Constants.Errors.ProductNotFound);
                existingDocuments = Db.Documents.Where(x => x.ExternalTransactionId == transactions.TransactionId &&
                    x.GameProviderId == transactions.GameProviderId && x.ProductId == product.Id).ToList();
                if (transactions.OperationTypeId == (int)OperationTypes.Win)
                    existingDocuments = existingDocuments.Where(x => x.OperationTypeId == (int)OperationTypes.Win).ToList();
            }
            else
            {
                existingDocuments = Db.Documents.Where(x => x.ExternalTransactionId == transactions.TransactionId &&
                    x.GameProviderId == transactions.GameProviderId && x.OperationTypeId == transactions.OperationTypeId).ToList();
            }

            if (!existingDocuments.Any())
                throw CreateException(LanguageId, Constants.Errors.DocumentNotFound);
            if (existingDocuments.Any(x => x.State == (int)BetDocumentStates.Deleted))
                throw CreateException(LanguageId, Constants.Errors.DocumentAlreadyRollbacked, integerInfo: existingDocuments[0].Id);

            if (existingDocuments.Any(
                    x =>
                        x.OperationTypeId == (int)OperationTypes.Rollback ||
                        x.OperationTypeId == (int)OperationTypes.BetRollback ||
                        x.OperationTypeId == (int)OperationTypes.WinRollback))
                throw CreateException(LanguageId, Constants.Errors.CanNotDeleteRollbackDocument);
            using (var scope = CommonFunctions.CreateTransactionScope())
            {                
                documents.AddRange(existingDocuments.Select(DeleteDocument));
                if (!string.IsNullOrEmpty(newExternalId))
                {
                    existingDocuments.ForEach(x =>
                    {
                        if (x.OperationTypeId == (int)OperationTypes.Win)
                            x.ExternalTransactionId = newExternalId;
                    });
                    Db.SaveChanges();
                }
                scope.Complete();
                var clients = existingDocuments.Where(x => x.ClientId.HasValue).Select(x => x.ClientId).ToList().Distinct();
                foreach (var c in clients)
                    CacheManager.RemoveClientBalance(c.Value);
                return documents;
            }
        }

        public Document GetDocumentByRoundId(int operationTypeId, string roundId, int providerId, int clientId, int?state =null)
        {
            var query = Db.Documents.Where(x => x.GameProviderId == providerId && x.OperationTypeId == operationTypeId && x.RoundId == roundId && x.ClientId == clientId);
            if (state.HasValue)
                query.Where(x => x.State == state);
            return query.FirstOrDefault();
        }

        public List<Document> GetDocumentsByRoundId(int operationTypeId, string roundId, int providerId, int clientId, int? state)
        {
            if (state == null)
                return Db.Documents.Where(x => x.GameProviderId == providerId && x.OperationTypeId == operationTypeId && x.RoundId == roundId && x.ClientId == clientId).ToList();
            else
                return Db.Documents.Where(x => x.GameProviderId == providerId && x.OperationTypeId == operationTypeId && x.RoundId == roundId && x.ClientId == clientId && x.State == state).ToList();
        }

        public Document GetLastDocumentByExternalId(string externalTransactionId, int clientId, int gameProviderId, int partnerProductSettingId, int operationTypeId)
        {
            Document result = Db.Documents.OrderByDescending(x => x.Id).FirstOrDefault(x =>
                x.ExternalTransactionId.StartsWith(externalTransactionId) && x.ClientId == clientId &&
                x.GameProviderId == gameProviderId && x.PartnerProductId == partnerProductSettingId && x.OperationTypeId == operationTypeId);
            return result;
        }

        public Document GetDocumentByExternalId(string externalTransactionId, int clientId, int gameProviderId, int partnerProductSettingId, int operationTypeId)
        {
            return Db.Documents.FirstOrDefault(x => x.ExternalTransactionId == externalTransactionId &&
                x.ClientId == clientId && x.GameProviderId == gameProviderId &&
                x.PartnerProductId == partnerProductSettingId && x.OperationTypeId == operationTypeId);
        }

        public Document GetDocumentOnlyByExternalId(string externalTransactionId, int gameProviderId, int clientId, int operationTypeId)
        {
            return Db.Documents.FirstOrDefault(x => x.ExternalTransactionId == externalTransactionId &&
                x.GameProviderId == gameProviderId && x.ClientId == clientId && x.OperationTypeId == operationTypeId);
        }

        public Document GetDocumentByExternalId(string externalTransactionId, int gameProviderId, int productId, OperationTypes operationTypeId, int? parentId)
        {
            return Db.Documents.FirstOrDefault(x => x.ExternalTransactionId == externalTransactionId && x.ProductId == productId &&
                                                    x.GameProviderId == gameProviderId && x.ParentId == parentId &&
                                                    x.OperationTypeId == (int)operationTypeId);
        }

        public void UpdateDocumentExternalId(long documentId, string newExternalId)
        {
            Db.Documents.Where(d => d.Id == documentId).UpdateFromQuery(d => new Document { ExternalTransactionId = newExternalId });
        }

        public void UpdateDocumentExternalId(long documentId, string newExternalId, string info)
        {
            Db.Documents.Where(d => d.Id == documentId).UpdateFromQuery(d => new Document { ExternalTransactionId = newExternalId, Info = info });
        }

        public List<fnClientBonus> GetBonuses()
        {
            return Db.fn_ClientBonus(Identity.LanguageId).Where(x => x.ClientId == Identity.Id).ToList();
        }
        public fnClientBonus GetClientBonusById(int bonusId)
        {
            return Db.fn_ClientBonus(Identity.LanguageId).Where(x => x.ClientId == Identity.Id && x.BonusId == bonusId).FirstOrDefault();
        }

        public List<ClientBonu> GetClientBonuses(int clientId)
        {
            return Db.ClientBonus.Where(x => x.ClientId == clientId).ToList();
        }

        public List<Document> RollBackPaymentRequest(List<Document> existingDocuments)
        {
            var documents = new List<Document>();

            if (!existingDocuments.Any())
                throw CreateException(LanguageId, Constants.Errors.DocumentNotFound);
            if (existingDocuments.Any(x => x.State == (int)BetDocumentStates.Deleted))
                throw CreateException(LanguageId, Constants.Errors.DocumentAlreadyRollbacked);

            if (existingDocuments.Any(x => x.OperationTypeId == (int)OperationTypes.DepositRollback))
                throw CreateException(LanguageId, Constants.Errors.CanNotDeleteRollbackDocument);
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                documents.AddRange(existingDocuments.Select(existingDocument => DeleteDocument(existingDocument)));
                scope.Complete();
                return documents;
            }
        }

        public Document CreateBonusDocumnet(Client client, decimal bonusPrice, int bonusOperationType, int accountTypeId)
        {
            if (!Enum.IsDefined(typeof(AccountTypes), accountTypeId) && !Enum.IsDefined(typeof(OperationTypes), accountTypeId))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var doc = new Document
            {
                Amount = bonusPrice,
                CurrencyId = client.CurrencyId,
                State = (int)BetDocumentStates.Paid,
                OperationTypeId = bonusOperationType,
                TypeId = (int)TransactionTypes.Debit,
                ClientId = client.Id,
                CreationTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.UtcNow
            };
            var operation = new Operation
            {
                Type = (int)OperationTypes.WelcomeBonus,
                Info = doc.Info,
                ClientId = doc.ClientId,
                Amount = doc.Amount,
                CurrencyId = doc.CurrencyId,
                ExternalTransactionId = doc.ExternalTransactionId,
                OperationItems = new List<OperationItem>()
            };

            var item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.ClientUnusedBalance,
                ObjectId = client.Id,
                ObjectTypeId = (int)ObjectTypes.Client,
                Amount = doc.Amount,
                CurrencyId = doc.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.WelcomeBonus
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.PartnerBalance,
                ObjectId = client.PartnerId,
                ObjectTypeId = (int)ObjectTypes.Partner,
                Amount = doc.Amount,
                CurrencyId = doc.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.WelcomeBonus
            };
            operation.OperationItems.Add(item);
            var document = CreateDocument(operation);
            Db.SaveChanges();
            return document;
        }

        public List<ClientBonu> FinalizeWageringBonusDocument()
        {
            var currentTime = DateTime.UtcNow;
            var resultList = new List<ClientBonu>();
            var dbClientBonuses = Db.ClientBonus.Include(x => x.Bonus).Where(x => x.Bonus.BonusType != (int)BonusTypes.CompaignFreeSpin &&
                                  (x.Status == (int)BonusStatuses.Finished || x.Status == (int)BonusStatuses.Lost)).Take(1000).ToList();
            var currencies = Db.Currencies.ToDictionary(x => x.Id, x => x.CurrentRate);
            foreach (var clientBonus in dbClientBonuses)
            {
                clientBonus.Status = clientBonus.Status == (int)BonusStatuses.Finished ? (int)BonusStatuses.Closed : (int)BonusStatuses.Expired;
                clientBonus.CalculationTime = DateTime.UtcNow;
                if (clientBonus.Bonus.BonusType != (int)BonusTypes.CompaignFreeSpin && clientBonus.Bonus.MaxReceiversCount.HasValue)
                    ++clientBonus.Bonus.TotalReceiversCount;
                if (clientBonus.Bonus.BonusType == (int)BonusTypes.CampaignWagerCasino || clientBonus.Bonus.BonusType == (int)BonusTypes.CampaignWagerSport)
                {
                    decimal bonusBalance = 0;
                    if (clientBonus.FinalAmount == null)
                    {
                        var bonusAccount = Db.Accounts.FirstOrDefault(x => x.ObjectTypeId == (int)ObjectTypes.Client && x.ObjectId == clientBonus.ClientId &&
                                                                           x.TypeId == (int)AccountTypes.ClientBonusBalance);
                        if (bonusAccount != null)
                            bonusBalance = bonusAccount.Balance;

                        clientBonus.FinalAmount = clientBonus.Status == (int)BonusStatuses.Expired ? 0 : (clientBonus.Bonus.Percent != null || clientBonus.Bonus.Percent > 0 ?
                            clientBonus.BonusPrize * clientBonus.Bonus.Percent / 100 : bonusBalance);
                    }
                    var client = Db.Clients.Include(x => x.Partner).FirstOrDefault(x => x.Id == clientBonus.ClientId);
                    if (clientBonus.Bonus.MaxAmount != null)
                        clientBonus.FinalAmount = Math.Min(clientBonus.FinalAmount.Value, ConvertCurrencyForJob(client.Partner.CurrencyId, currencies[client.Partner.CurrencyId],
                            client.CurrencyId, currencies[client.CurrencyId], clientBonus.Bonus.MaxAmount.Value));
                    if (bonusBalance > 0 || clientBonus.FinalAmount > 0)
                    {
                        if (clientBonus.Bonus.Info != "1")
                        {
                            var input = new Operation
                            {
                                Amount = Math.Max(clientBonus.FinalAmount.Value, bonusBalance),
                                CurrencyId = client.CurrencyId,
                                Type = (int)OperationTypes.BonusWin,
                                ClientId = client.Id,
                                OperationItems = new List<OperationItem>()
                            };
                            if (bonusBalance > 0)
                            {
                                input.OperationItems.Add(new OperationItem
                                {
                                    AccountTypeId = (int)AccountTypes.ClientBonusBalance,
                                    ObjectId = client.Id,
                                    ObjectTypeId = (int)ObjectTypes.Client,
                                    Amount = bonusBalance,
                                    CurrencyId = client.CurrencyId,
                                    Type = (int)TransactionTypes.Credit,
                                    OperationTypeId = (int)OperationTypes.BonusWin
                                });
                            }
                            if (clientBonus.FinalAmount.Value > 0)
                            {
                                input.OperationItems.Add(new OperationItem
                                {
                                    AccountTypeId = clientBonus.Bonus.AccountTypeId == null || clientBonus.Bonus.AccountTypeId == (int)AccountTypes.ClientBonusBalance ?
                                                    (int)AccountTypes.ClientUnusedBalance : clientBonus.Bonus.AccountTypeId.Value,
                                    ObjectId = client.Id,
                                    ObjectTypeId = (int)ObjectTypes.Client,
                                    Amount = clientBonus.FinalAmount.Value,
                                    CurrencyId = client.CurrencyId,
                                    Type = (int)TransactionTypes.Debit,
                                    OperationTypeId = (int)OperationTypes.BonusWin
                                });
                            }
                            if (bonusBalance != clientBonus.FinalAmount.Value)
                                input.OperationItems.Add(new OperationItem
                                {
                                    AccountTypeId = (int)AccountTypes.PartnerBalance,
                                    ObjectId = client.PartnerId,
                                    ObjectTypeId = (int)ObjectTypes.Partner,
                                    Amount = Math.Abs(bonusBalance - clientBonus.FinalAmount.Value),
                                    CurrencyId = client.CurrencyId,
                                    Type = bonusBalance > clientBonus.FinalAmount.Value ? (int)TransactionTypes.Debit : (int)TransactionTypes.Credit,
                                    OperationTypeId = (int)OperationTypes.BonusWin
                                });

                            var document = CreateDocument(input);
                            clientBonus.Bonus.TotalGranted += ConvertCurrencyForJob(client.CurrencyId, currencies[client.CurrencyId],
                                                                                       client.Partner.CurrencyId, currencies[client.Partner.CurrencyId],
                                                                                       input.Amount);
                        }
                        else
                        {
                            var clientSegmentsIds = new List<int>();
                            var clientClasifications = CacheManager.GetClientClasifications(client.Id);
                            if (clientClasifications.Any())
                                clientSegmentsIds = clientClasifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                                        .Select(x => x.SegmentId.Value).ToList();
                            var bonuses = Db.Bonus.Include(x => x.TriggerGroups
                                                  .Select(y => y.TriggerGroupSettings))
                                                  .Where(x => x.Status && x.StartTime < currentTime && x.FinishTime > currentTime &&
                                                     x.PartnerId == client.PartnerId &&
                                                   (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                                   (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount) &&
                                                   (!x.BonusSegmentSettings.Any() ||
                                                     x.BonusSegmentSettings.Any(y => (y.Type == (int)BonusSettingConditionTypes.InSet && clientSegmentsIds.Contains(y.SegmentId))) ||
                                                    !x.BonusSegmentSettings.Any(y => (y.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegmentsIds.Contains(y.SegmentId)))) &&
                                                   (!x.BonusCountrySettings.Any() ||
                                                     x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId == client.RegionId) ||
                                                    !x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CountryId == client.RegionId)
                                                   ) &&
                                                   (!x.BonusCurrencySettings.Any() ||
                                                     x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CurrencyId == client.CurrencyId) ||
                                                    !x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CurrencyId == client.CurrencyId)) &&
                                                   (!x.BonusLanguageSettings.Any() ||
                                                     x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.LanguageId == client.LanguageId) ||
                                                    !x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.LanguageId == client.LanguageId)
                                                   )).ToList();
                            var claimedBonuses = new List<int>();
                            var date = (long)currentTime.Year * 100000000 + currentTime.Month * 1000000 + currentTime.Day * 10000 + currentTime.Hour * 100 + currentTime.Minute;
                            foreach (var b in bonuses)
                            {
                                var bon = bonuses.FirstOrDefault(x => x.Id == b.Id).TriggerGroups.FirstOrDefault(x => x.Priority == 0 && x.TriggerGroupSettings.Count() == 1);
                                if (bon != null)
                                {
                                    var sId = bon.TriggerGroupSettings.First().SettingId;
                                    var setting = Db.TriggerSettings.First(x => x.Id == sId);
                                    if (setting.Type == (int)TriggerTypes.CampainLinkCode && setting.BonusSettingCodes == clientBonus.BonusId.ToString() &&
                                        setting.StartTime <= currentTime && setting.FinishTime > currentTime)
                                    {
                                        var dbBonuses = Db.ClientBonus.Where(x => x.ClientId == clientBonus.ClientId && x.BonusId == b.Id).ToList();
                                        var maxNumber = dbBonuses.Any() ? dbBonuses.Max(x => x.ReuseNumber) : 0;
                                        if (maxNumber < (b.ReusingMaxCount ?? 1))
                                        {
                                            var triggerMinAmount = setting.MinAmount.HasValue ? ConvertCurrencyForJob(client.Partner.CurrencyId, currencies[client.Partner.CurrencyId],
                                                client.CurrencyId, currencies[client.CurrencyId], setting.MinAmount.Value) : (decimal?)null;
                                            var triggerMaxAmount = setting.MaxAmount.HasValue ? ConvertCurrencyForJob(client.Partner.CurrencyId, currencies[client.Partner.CurrencyId],
                                                client.CurrencyId, currencies[client.CurrencyId], setting.MaxAmount.Value) : (decimal?)null;

                                            var amount = setting.Percent > 0 ? clientBonus.FinalAmount * setting.Percent / 100 : (triggerMinAmount ?? 0);
                                            if ((triggerMinAmount == null || amount >= triggerMinAmount.Value))
                                            {
                                                if (triggerMaxAmount != null && amount > triggerMaxAmount.Value)
                                                    amount = triggerMaxAmount.Value;
                                                Db.ClientBonus.Add(new ClientBonu
                                                {
                                                    BonusId = b.Id,
                                                    ClientId = clientBonus.ClientId,
                                                    Status = (int)BonusStatuses.NotAwarded,
                                                    CreationTime = currentTime,
                                                    CreationDate = date,
                                                    ValidUntil = b.ValidForAwarding == null ? (DateTime?)null : currentTime.AddHours(b.ValidForAwarding.Value),
                                                    CalculationTime = (DateTime?)null,
                                                    ReuseNumber = maxNumber + 1
                                                });
                                                Db.ClientBonusTriggers.Add(new ClientBonusTrigger
                                                {
                                                    ClientId = clientBonus.ClientId,
                                                    TriggerId = setting.Id,
                                                    BonusId = b.Id,
                                                    SourceAmount = amount,
                                                    CreationTime = currentTime,
                                                    ReuseNumber = maxNumber + 1
                                                });
                                                Db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (clientBonus.Bonus.BonusType == (int)BonusTypes.CampaignCash)
                {
                    if (clientBonus.FinalAmount == null)
                    {
                        clientBonus.FinalAmount = clientBonus.Status == (int)BonusStatuses.Expired ? 0 : (clientBonus.Bonus.Percent != null || clientBonus.Bonus.Percent > 0 ?
                            clientBonus.BonusPrize * clientBonus.Bonus.Percent / 100 : 0);
                    }
                    var client = Db.Clients.Include(x => x.Partner).FirstOrDefault(x => x.Id == clientBonus.ClientId);
                    if (clientBonus.Bonus.MaxAmount != null)
                        clientBonus.FinalAmount = Math.Min(clientBonus.FinalAmount.Value, ConvertCurrencyForJob(client.Partner.CurrencyId, currencies[client.Partner.CurrencyId],
                            client.CurrencyId, currencies[client.CurrencyId], clientBonus.Bonus.MaxAmount.Value));
                    if (clientBonus.FinalAmount > 0)
                    {
                        var input = new Operation
                        {
                            Amount = clientBonus.FinalAmount.Value,
                            CurrencyId = client.CurrencyId,
                            Type = (int)OperationTypes.BonusWin,
                            ClientId = client.Id,
                            OperationItems = new List<OperationItem>()

                        };
                        input.OperationItems.Add(new OperationItem
                        {
                            AccountTypeId = (int)AccountTypes.PartnerBalance,
                            ObjectId = client.PartnerId,
                            ObjectTypeId = (int)ObjectTypes.Partner,
                            Amount = clientBonus.FinalAmount.Value,
                            CurrencyId = client.CurrencyId,
                            Type = (int)TransactionTypes.Credit,
                            OperationTypeId = (int)OperationTypes.BonusWin
                        });
                        input.OperationItems.Add(new OperationItem
                        {
                            AccountTypeId = clientBonus.Bonus.AccountTypeId == null ?
                                    (int)AccountTypes.ClientUnusedBalance : clientBonus.Bonus.AccountTypeId.Value,
                            ObjectId = client.Id,
                            ObjectTypeId = (int)ObjectTypes.Client,
                            Amount = clientBonus.FinalAmount.Value,
                            CurrencyId = client.CurrencyId,
                            Type = (int)TransactionTypes.Debit,
                            OperationTypeId = (int)OperationTypes.BonusWin
                        });
                        var document = CreateDocument(input);
                    }
                }
                resultList.Add(clientBonus);
            }
            Db.SaveChanges();
            return resultList;
        }

        public Document CreateDocumentFromJob(Operation operation)
        {
            if (operation.Amount < 0)
                throw CreateException(LanguageId, Constants.Errors.WrongOperationAmount);
            var currentDate = GetServerDate();
            var date = (long)currentDate.Year * 1000000 + (long)currentDate.Month * 10000 + (long)currentDate.Day * 100 + (long)currentDate.Hour;
            var document = new Document
            {
                ExternalTransactionId = operation.ExternalTransactionId,
                Amount = operation.Amount,
                CurrencyId = operation.CurrencyId,
                ExternalOperationId = operation.ExternalOperationId,
                TicketNumber = operation.TicketNumber,
                TicketInfo = operation.TicketInfo,
                CashDeskId = operation.CashDeskId,
                PartnerPaymentSettingId = operation.PartnerPaymentSettingId,
                PaymentRequestId = operation.PaymentRequestId,
                PartnerProductId = operation.PartnerProductId,
                ProductId = operation.ProductId,
                GameProviderId = operation.GameProviderId,
                SessionId = Identity.IsAdminUser ? Identity.SessionId : operation.SessionId,
                ClientId = operation.ClientId,
                Info = operation.Info,
                ParentId = operation.ParentId,
                OperationTypeId = operation.Type,
                UserId = operation.UserId,
                Creator = operation.Creator,
                DeviceTypeId = operation.DeviceTypeId,
                TypeId = operation.DocumentTypeId,
                State = operation.State ?? (int)BetDocumentStates.Uncalculated,
                RoundId = operation.RoundId,
                PossibleWin = operation.PossibleWin,
                CreationTime = currentDate,
                LastUpdateTime = currentDate,
                Date = date
            };
            var debitTrans =
               operation.OperationItems.Where(x => x.Type == (int)TransactionTypes.Debit).ToList();
            var creditTrans =
                operation.OperationItems.Where(x => x.Type == (int)TransactionTypes.Credit).ToList();
            document.Transactions = new List<Transaction>();
            foreach (var operationItem in debitTrans)
            {
                document.Transactions.Add(CreateDebitTransactionFromJob(operationItem));
            }
            foreach (var operationItem in creditTrans)
            {
                var transactions = CreateCreditTransactionFromJob(operationItem);
                transactions.ForEach(x => document.Transactions.Add(x));
            }
            Db.Documents.Add(document);
            return document;
        }

        private Transaction CreateDebitTransactionFromJob(OperationItem operationItem)
        {
            bool willBalanceChange = (operationItem.ObjectTypeId != (int)ObjectTypes.Partner && operationItem.ObjectTypeId != (int)ObjectTypes.PartnerProduct);
            Account account = null;
            if (operationItem.AccountId.HasValue)
            {
                if (willBalanceChange)
                {
                    Db.Procedures.sp_GetAccountLockByIdAsync(operationItem.AccountId.Value).Wait();
                    account = GetAccount(operationItem.AccountId.Value);
                }
                else
                {
                    account = new Account { Id = operationItem.AccountId.Value };
                }
            }
            else
            {
                if (!operationItem.AccountTypeId.HasValue)
                    throw CreateException(LanguageId, Constants.Errors.AccountNotFound);

                var accountId = CreateAccountIfNotExists(operationItem.ObjectId, operationItem.ObjectTypeId,
                    operationItem.CurrencyId, operationItem.AccountTypeId.Value, false);

                if (willBalanceChange)
                {
                    Db.Procedures.sp_GetAccountLockByIdAsync(accountId).Wait();
                    account = GetAccount(accountId);
                }
                else
                {
                    account = new Account { Id = accountId };
                }
            }
            var currentDate = GetServerDate();
            var transaction = new Transaction
            {
                AccountId = account.Id,
                Amount = operationItem.Amount,
                OperationTypeId = operationItem.OperationTypeId,
                Type = operationItem.Type,
                CreationTime = currentDate,
                Date = currentDate.Year * 1000000 + currentDate.Month * 10000 + currentDate.Day * 100 + currentDate.Hour
            };
            if (operationItem.Amount > 0)
            {
                if (willBalanceChange)
                {
                    Db.Entry(account).Reload();
                    ChangeAccountBalanceForJob(operationItem.Amount, account);
                }
            }
            return transaction;
        }

        public List<Transaction> CreateCreditTransactionFromJob(OperationItem operationItem)
        {
            var currentTime = GetServerDate();
            var transactions = new List<Transaction>();
            bool willBalanceChange = (operationItem.ObjectTypeId != (int)ObjectTypes.Partner && operationItem.ObjectTypeId != (int)ObjectTypes.PartnerProduct);

            if (willBalanceChange)
            {
                Account account = null;
                if (operationItem.AccountId.HasValue)
                {
                    Db.Procedures.sp_GetAccountLockByIdAsync(operationItem.AccountId).Wait();
                    account = GetAccount(operationItem.AccountId.Value);
                }
                else
                {
                    Db.Procedures.sp_GetAccountLockAsync(operationItem.ObjectId, operationItem.ObjectTypeId,
                        operationItem.CurrencyId, operationItem.AccountTypeId).Wait();
                    account = GetOrCreateAccount(operationItem.ObjectId, operationItem.ObjectTypeId,
                        operationItem.CurrencyId, operationItem.AccountTypeId.Value, false);
                }

                var accountType = Db.AccountTypes.First(x => x.Id == account.TypeId);
                if (account.Balance - operationItem.Amount < 0 && !accountType.CanBeNegative)
                    throw CreateException(LanguageId, Constants.Errors.LowBalance);

                var transaction = new Transaction
                {
                    AccountId = account.Id,
                    Amount = operationItem.Amount,
                    OperationTypeId = operationItem.OperationTypeId,
                    Type = operationItem.Type,
                    CreationTime = currentTime,
                    Date = currentTime.Year * 1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour,
                    AccountTypeId = account.TypeId
                };
                transactions.Add(transaction);
                ChangeAccountBalanceForJob(operationItem.Amount * -1, account);
            }
            else
            {
                long accountId = 0;
                if (operationItem.AccountId.HasValue)
                    accountId = operationItem.AccountId.Value;
                else
                {
                    var account = GetOrCreateAccount(operationItem.ObjectId, operationItem.ObjectTypeId,
                        operationItem.CurrencyId, operationItem.AccountTypeId.Value, false);
                    accountId = account.Id;
                }

                var transaction = new Transaction
                {
                    AccountId = accountId,
                    Amount = operationItem.Amount,
                    OperationTypeId = operationItem.OperationTypeId,
                    Type = operationItem.Type,
                    CreationTime = currentTime,
                    Date = currentTime.Year * 1000000 + currentTime.Month * 10000 + currentTime.Day * 100 + currentTime.Hour
                };
                transactions.Add(transaction);
            }

            return transactions;
        }
    }
}