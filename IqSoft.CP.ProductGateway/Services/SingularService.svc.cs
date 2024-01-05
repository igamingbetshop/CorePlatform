using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Singular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SingularService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SingularService.svc or SingularService.svc.cs at the Solution Explorer and start debugging.
    public class SingularService : ISingularService
    {
        public AuthenticationOutput AuthenticateUserByToken(string operatorId, string token, string hash)
        {
            AuthenticationOutput response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                try
                {
                    if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                    var partner = SingularHelpers.OperatorIds[operatorId];

                    var message = string.Format("{0}{1}", operatorId, token);
                    CheckSign(message, hash, partner);
                    var clientSession = CheckClientSession(token);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
                    if (product.GameProviderId != providerId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);

                    var currency = CacheManager.GetCurrencyById(client.CurrencyId);

                    response = new AuthenticationOutput
                    {
                        ResponseCode = SingularHelpers.ErrorCodes.Success,
                        UserId = client.Id,
                        UserName = client.UserName,
                        UserIp = clientSession.LoginIp,
                        PreferredCurrencyId = Convert.ToInt16(SingularHelpers.Currencies.First(x => x.Value == currency.Id).Key)
                    };
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(fex));
                    var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                    response = new AuthenticationOutput
                    {
                        ResponseCode = error
                    };
                }
                catch (Exception ex)
                {
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                    var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                    response = new AuthenticationOutput
                    {
                        ResponseCode = error
                    };
                }
            }
            return response;
        }

        public AmountResponse GetBalance(string operatorId, long userId, short currencyCode, bool isSingle, string hash)
        {
            AmountResponse response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(partnerBl))
                {
                    try
                    {
                        if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                        var partner = SingularHelpers.OperatorIds[operatorId];

                        string message = string.Format("{0}{1}{2}{3}", operatorId, userId, currencyCode, isSingle ? "true" : "false");
                        CheckSign(message, hash, partner);
                        int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
                        var client = CacheManager.GetClientById((int)userId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        var product = CacheManager.GetProductByExternalId(providerId,
                            ((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());

                        ClientBll.GetClientSessionByProductId(client.Id, product.Id);

                        var currency = CacheManager.GetCurrencyById(SingularHelpers.Currencies[currencyCode]);
                        if (currency == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CurrencyNotExists);

                        response = new AmountResponse()
                        {
                            StatusCode = SingularHelpers.ErrorCodes.Success,
                            Amount = GetBalanceAmount(clientBl, (int)userId, currency.Id, product.Id)
                        };
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(fex));
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = new AmountResponse
                        {
                            StatusCode = error
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                        var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                        response = new AmountResponse
                        {
                            StatusCode = error
                        };
                    }
                }
            }
            return response;
        }

        public WithdrawOutput WithdrawMoney(string operatorId, long userId, short currencyCode, decimal amount, bool shouldWaitForApproval,
            string providerUserId, int? providerServiceId, string transactionId, string additionalData, string providerStatusCode,
            string statusNote, string hash)
        {
            WithdrawOutput response;
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    try
                    {
                        if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                        int partner = SingularHelpers.OperatorIds[operatorId];

                        var message = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}", operatorId, userId, currencyCode, amount,
                            shouldWaitForApproval ? "true" : "false", providerUserId, providerServiceId, transactionId, additionalData,
                            providerStatusCode, statusNote);
                        CheckSign(message, hash, partner);
                        var client = CacheManager.GetClientById((int)userId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
                        var product = CacheManager.GetProductByExternalId(providerId,
                            ((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());
                        ClientBll.GetClientSessionByProductId(client.Id, product.Id);
                        var currency = CacheManager.GetCurrencyById(SingularHelpers.Currencies[currencyCode]);
                        if (currency == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CurrencyNotExists);
                        var betDocument = WriteTransaction(clientBl, currency.Id, transactionId, client,
                            amount, null, OperationTypes.Bet, partner, documentBl);
                        WriteTransaction(clientBl, currency.Id, transactionId, client,
                            0, betDocument, OperationTypes.Win, partner, documentBl);

                        response = new WithdrawOutput()
                        {
                            ResponseCode = SingularHelpers.ErrorCodes.Success,
                            TransactionId = betDocument.Id.ToString(),
                            TotalAmount = amount
                        };
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(fex));
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = new WithdrawOutput
                        {
                            ResponseCode = error
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                        var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                        response = new WithdrawOutput
                        {
                            ResponseCode = error
                        };
                    }
                }
            }
            return response;
        }

        public GenericOutput DepositMoney(string operatorId, long userId, short currencyCode, decimal amount, bool isCardVerification,
            bool shouldWaitForApproval, string providerUserId, int? providerServiceId, string transactionId, string additionalData,
            string requestorIp, string statusNote, string hash)
        {
            GenericOutput response;
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    try
                    {
                        if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                        var partner = SingularHelpers.OperatorIds[operatorId];

                        var message = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", operatorId, userId, currencyCode, amount,
                            isCardVerification ? "true" : "false", shouldWaitForApproval ? "true" : "false", providerUserId, providerServiceId,
                            transactionId, additionalData, requestorIp, statusNote);
                        CheckSign(message, hash, partner);
                        var client = CacheManager.GetClientById((int)userId);
                        var currency = CacheManager.GetCurrencyById(SingularHelpers.Currencies[currencyCode]);
                        if (currency == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CurrencyNotExists);
                        if (amount <= 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        var betDocument = WriteTransaction(clientBl, currency.Id, transactionId, client,
                             0, null, OperationTypes.Bet, partner, documentBl);
                        var winDocument = WriteTransaction(clientBl, currency.Id, transactionId, client,
                            amount, betDocument, OperationTypes.Win, partner, documentBl);

                        response = new GenericOutput
                        {
                            ResponseCode = SingularHelpers.ErrorCodes.Success,
                            TransactionId = winDocument.Id.ToString()
                        };
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(fex));
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = new GenericOutput
                        {
                            ResponseCode = error
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                        var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                        response = new GenericOutput
                        {
                            ResponseCode = error
                        };
                    }
                }
            }
            return response;
        }

        public GenericOutput CheckTransactionStatus(string operatorId, string transactionId, bool isCoreTransactionId, string hash)
        {
            GenericOutput response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(partnerBl))
                {
                    try
                    {
                        if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                        var partner = SingularHelpers.OperatorIds[operatorId];

                        string message = string.Format("{0}{1}{2}", operatorId, transactionId, isCoreTransactionId ? "true" : "false");
                        CheckSign(message, hash, partner);
                        int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;

                        string returnTransactionId;
                        if (isCoreTransactionId)
                        {
                            var returnTransaction = documentBl.GetDocumentById(long.Parse(transactionId));
                            if (returnTransaction == null)
                            {
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                            }
                            var resp = returnTransaction.ExternalTransactionId.Split('_');

                            returnTransactionId = resp[resp.Length - 1];
                        }
                        else
                        {
                            var returnTransaction = documentBl.GetDocumentOnlyByExternalId(string.Format("{0}_{1}", partner, transactionId), providerId, 0,0);
                            if (returnTransaction == null)
                            {
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                            }
                            returnTransactionId = returnTransaction.Id.ToString();
                        }

                        response = new GenericOutput
                        {
                            ResponseCode = SingularHelpers.ErrorCodes.TransactionStatusSuccess,
                            TransactionId = returnTransactionId
                        };
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(fex));
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = new GenericOutput
                        {
                            ResponseCode = error
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                        var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                        response = new GenericOutput
                        {
                            ResponseCode = error
                        };
                    }
                }
            }
            return response;
        }

        public int RollbackTransaction(string operatorId, string transactionOfProviderId, string transactionId, bool isCoreTransactionId,
            string statusNote, string hash)
        {
            int response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(partnerBl))
                {
                    try
                    {
                        if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                        var partner = SingularHelpers.OperatorIds[operatorId];

                        var message = string.Format("{0}{1}{2}{3}{4}", operatorId, transactionOfProviderId, transactionId,
                            isCoreTransactionId ? "true" : "false", statusNote);
                        CheckSign(message, hash, partner);
                        int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
                        var product = CacheManager.GetProductByExternalId(providerId,
                            ((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            GameProviderId = providerId,
                            ProductId = product.Id,
                            TransactionId = string.Format("{0}_{1}", partner, transactionId)
                        };
                        if (isCoreTransactionId)
                        {
                            Document document = documentBl.GetDocumentById(long.Parse(transactionId));
                            operationsFromProduct.TransactionId = document.ExternalTransactionId;
                        }

                        documentBl.RollbackProductTransactions(operationsFromProduct);
                      //  BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        response = SingularHelpers.ErrorCodes.Success;
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(fex));
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = error;
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                        var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                        response = error;
                    }
                }
            }
            return response;
        }

        public ExchangeRateOutput[] GetExchangeRates(string operatorId, string hash)
        {
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                try
                {
                    if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                    var partner = SingularHelpers.OperatorIds[operatorId];

                    CheckSign(operatorId, hash, partner);
                    var response = new ExchangeRateOutput[SingularHelpers.Currencies.Count];
                    BllCurrency currency = null;
                    for (var i = 0; i < SingularHelpers.Currencies.Count; i++)
                    {
                        currency = CacheManager.GetCurrencyById(SingularHelpers.Currencies[i + 2]);
                        response[i] = new ExchangeRateOutput()
                        {
                            CurrencyId = Convert.ToInt16(SingularHelpers.Currencies.First(x => x.Value == currency.Id).Key),
                            BuyRate = (long)(currency.CurrentRate * 10000) / 10000m,
                            SellRate = (long)(currency.CurrentRate * 10000) / 10000m,
                            ModificationDate = currency.LastUpdateTime
                        };
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                    return new ExchangeRateOutput[0];
                }
            }
        }

        public AmountResponse Exchange(string operatorId, int sourceCurrencyId, int destinationCurrencyId, decimal amount,
            bool isReverse, string hash)
        {
            AmountResponse response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                try
                {
                    if (!SingularHelpers.OperatorIds.ContainsKey(operatorId))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                    var partner = SingularHelpers.OperatorIds[operatorId];

                    var message = string.Format("{0}{1}{2}{3}", operatorId, sourceCurrencyId, destinationCurrencyId, amount);
                    CheckSign(message, hash, partner);

                    decimal convertedAmount = 0m;

                    if (!isReverse)
                        convertedAmount = ExchangeAmount(sourceCurrencyId, destinationCurrencyId, amount);
                    else
                        convertedAmount = ExchangeAmount(destinationCurrencyId, sourceCurrencyId, amount);

                    response = new AmountResponse()
                    {
                        StatusCode = SingularHelpers.ErrorCodes.Success,
                        Amount = (long)convertedAmount
                    };
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(fex));
                    var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                    response = new AmountResponse
                    {
                        StatusCode = error
                    };
                }
                catch (Exception ex)
                {
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(ex));
                    var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                    response = new AmountResponse
                    {
                        StatusCode = error
                    };
                }
            }
            return response;
        }

        private void CheckSign(string strParams, string sign, int partnerId)
        {
            var gameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.SingularSecretKey);
            if (partnerKey == null || partnerKey == string.Empty)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
            var ourHash = CommonFunctions.ComputeMd5(strParams + partnerKey);
            if (ourHash != sign)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }

        private SessionIdentity CheckClientSession(string token)
        {
            return ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
        }

		private decimal GetBalanceAmount(ClientBll clientBl, int clientId, string currency, int productId)
		{
			var balance = BaseHelpers.GetClientProductBalance(clientId, productId);
			return (long)(balance * 100m);
		}

		private Document WriteTransaction(ClientBll clientBl, string currencyId,
			string transactionId, BllClient client, decimal amount, Document betDocument, OperationTypes transactionType, int partnerId, DocumentBll documentBl)
		{
			int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
			var product = CacheManager.GetProductByExternalId(providerId,
				((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());

			var operationsFromProduct = new ListOfOperationsFromApi
			{
				CurrencyId = currencyId,
				//RoundId = additionalData,
				GameProviderId = providerId,
				ProductId = product.Id,
				TransactionId = string.Format("{0}_{1}", partnerId, transactionId),
				OperationItems = new List<OperationItemFromProduct>()
			};
			operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
			{
				Client = client,
				Amount = amount / 100
			});

			Document document = null;
            if (transactionType == OperationTypes.Bet)
            {
                document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                BaseHelpers.BroadcastBetLimit(info);
            }
            else
            {
                if (Math.Abs(Math.Abs(amount) - 0) < Constants.Delta)
                    betDocument.State = (int)BetDocumentStates.Lost;
                else if (amount > 0)
                    betDocument.State = (int)BetDocumentStates.Won;
                operationsFromProduct.CreditTransactionId = betDocument.Id;
                document = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl).FirstOrDefault();
            }
            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            return document;
		}

        private decimal ExchangeAmount(int fromCurrency, int toCurrency, decimal amount)
        {
            var initialCurrency = CacheManager.GetCurrencyById(SingularHelpers.Currencies[fromCurrency]);
            var destinationCurrency = CacheManager.GetCurrencyById(SingularHelpers.Currencies[toCurrency]);
            var convertedAmount = amount * initialCurrency.CurrentRate / destinationCurrency.CurrentRate;
            return convertedAmount;
        }
    }
}