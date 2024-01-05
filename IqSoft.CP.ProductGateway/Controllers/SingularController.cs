using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Singular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web.Http;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class SingularController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;

        [HttpPost]
        [Route("{partnerId}/api/Singular/authenticateUserByToken")]
        public AuthenticationOutput AuthenticateUserByToken(int partnerId, AuthenticationInput input)
        {
            AuthenticationOutput response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                try
                {
					if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
					var partner = SingularHelpers.OperatorIds[input.OperatorId];

					var message = input.OperatorId + input.Token;
                    CheckSign(message, partnerBl, input.Hash, partner);
                    var clientSession = CheckClientSession(input.Token);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
					if (client.PartnerId != partnerId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);
                    if (product.GameProviderId != providerId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);

                    var currency = CacheManager.GetCurrencyById(client.CurrencyId);

                    response = new AuthenticationOutput
                    {
                        ResponseCode = SingularHelpers.ErrorCodes.Success,
                        UserId = client.Id,
                        UserName = client.UserName,
                        UserIp = clientSession.LoginIp,
                        PreferredCurrencyId = Convert.ToInt16(currency.Code)
                    };
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                    response = new AuthenticationOutput
                    {
                        ResponseCode = error
                    };
                }
                catch (Exception ex)
                {
                    WebApiApplication.DbLogger.Error(ex);
                    var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                    response = new AuthenticationOutput
                    {
                        ResponseCode = error
                    };
                }
            }
            return response;
        }

        [HttpPost]
        [Route("{partnerId}/api/Singular/getBalance")]
        public AmountResponse GetBalance(int partnerId, BalanceInput input)
        {
            AmountResponse response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(partnerBl))
                {
                    try
                    {
						if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
						var partner = SingularHelpers.OperatorIds[input.OperatorId];

						string message = input.OperatorId + input.UserId + input.CurrencyId + input.IsSingle;
                        CheckSign(message, partnerBl, input.Hash, partner);
                        var client = CacheManager.GetClientById((int)input.UserId);
						
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        if (partnerId != client.PartnerId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);

                        response = new AmountResponse()
                        {
                            StatusCode = SingularHelpers.ErrorCodes.Success,
                            Amount = GetBalanceAmount(documentBl, (int)input.UserId, input.CurrencyId)
                        };
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = new AmountResponse
                        {
                            StatusCode = error
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
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

        [HttpPost]
        [Route("{partnerId}/api/Singular/withdrawMoney")]
        public WithdrawOutput WithdrawMoney(int partnerId, WithdrawInput input)
        {
            WithdrawOutput response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                try
                {
                    if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
                    var partner = SingularHelpers.OperatorIds[input.OperatorId];

                    var message = input.OperatorId + input.UserId + input.CurrencyId + input.Amount +
                        input.ShouldWaitForApproval + input.ProviderUserId + input.ProviderServiceId +
                        input.TransactionId + input.AdditionalData + input.ProviderStatusCode + input.StatusNote;
                    CheckSign(message, partnerBl, input.Hash, partner);
                    var client = CacheManager.GetClientById((int)input.UserId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
                    var product = CacheManager.GetProductByExternalId(providerId,
                        ((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());

                    ClientBll.GetClientSessionByProductId(client.Id, product.Id);
                    if (partnerId != client.PartnerId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);

                    var betDocument = WriteTransaction(input, null, input.Amount, OperationTypes.Bet);
                    WriteTransaction(input, betDocument, 0, OperationTypes.Win);

                    response = new WithdrawOutput()
                    {
                        ResponseCode = SingularHelpers.ErrorCodes.Success,
                        TransactionId = betDocument.Id.ToString(),
                        TotalAmount = input.Amount
                    };
                }
                catch (FaultException<BllFnErrorType> fex)
                {
                    var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                    response = new WithdrawOutput
                    {
                        ResponseCode = error
                    };
                }
                catch (Exception ex)
                {
                    WebApiApplication.DbLogger.Error(ex);
                    var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                    response = new WithdrawOutput
                    {
                        ResponseCode = error
                    };
                }
            }
            return response;
        }

        [HttpPost]
        [Route("{partnerId}/api/Singular/depositMoney")]
        public GenericOutput DepositMoney(int partnerId, DepositInput input)
        {
            GenericOutput response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
				try
				{
					if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
					var partner = SingularHelpers.OperatorIds[input.OperatorId];

					var message = input.OperatorId + input.UserId + input.CurrencyId + input.Amount +
						input.IsCardVerification + input.ShouldWaitForApproval + input.ProviderUserId +
						input.ProviderServiceId + input.TransactionId + input.AdditionalData +
						input.RequestorIp + input.StatusNote;
					CheckSign(message, partnerBl, input.Hash, partner);
					var client = CacheManager.GetClientById((int)input.UserId);
					if (input.Amount <= 0)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
					if (client == null)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
					if (partnerId != client.PartnerId)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPartnerId);

					var betDocument = WriteTransaction(input, null, 0, OperationTypes.Bet);
					var winDocument = WriteTransaction(input, betDocument, input.Amount, OperationTypes.Win);

					response = new GenericOutput
					{
						ResponseCode = SingularHelpers.ErrorCodes.Success,
						TransactionId = winDocument.Id.ToString()
					};
				}
				catch (FaultException<BllFnErrorType> fex)
				{
					var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
					response = new GenericOutput
					{
						ResponseCode = error
					};
				}
				catch (Exception ex)
				{
                    WebApiApplication.DbLogger.Error(ex);
					var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
					response = new GenericOutput
					{
						ResponseCode = error
					};
				}
            }
            return response;
        }

        [HttpPost]
        [Route("{partnerId}/api/Singular/checkTransactionStatus")]
        public GenericOutput CheckTransactionStatus(int partnerId, TransactionStatusInput input)
        {
            GenericOutput response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(partnerBl))
                {
                    try
                    {
						if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
						var partner = SingularHelpers.OperatorIds[input.OperatorId];

						string message = input.OperatorId + input.TransactionId + input.IsCoreTransactionId;
                        CheckSign(message, partnerBl, input.Hash, partner);
                        int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
                        var product = CacheManager.GetProductByExternalId(providerId,
                            ((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                        string returnTransactionId;
                        if (input.IsCoreTransactionId)
                        {
                            var returnTransaction = documentBl.GetDocumentById(long.Parse(input.TransactionId));
                            if (returnTransaction == null)
                            {
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                            }
                            returnTransactionId = returnTransaction.ExternalTransactionId;
                        }
                        else
                        {
                            var returnTransaction = documentBl.GetDocumentOnlyByExternalId(input.TransactionId, providerId, 0, 0); // must be changed
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
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = new GenericOutput
                        {
                            ResponseCode = error
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
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

        [HttpPost]
        [Route("{partnerId}/api/Singular/rollbackTransaction")]
        public BaseOutput RollbackTransaction(int partnerId, RollbackInput input)
        {
            BaseOutput response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(partnerBl))
                {
                    try
                    {
						if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
						var partner = SingularHelpers.OperatorIds[input.OperatorId];

						var message = input.OperatorId + input.TransactionOfProviderId + input.TransactionId +
                            input.IsCoreTransactionId + input.StatusNote;
                        CheckSign(message, partnerBl, input.Hash, partner);
                        int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
                        var product = CacheManager.GetProductByExternalId(providerId,
                            ((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            GameProviderId = providerId,
                            ProductId = product.Id,
                            TransactionId = input.TransactionId
                        };
                        if (input.IsCoreTransactionId)
                        {
                            Document document = documentBl.GetDocumentById(long.Parse(input.TransactionId));
                            operationsFromProduct.TransactionId = document.ExternalTransactionId;
                        }

                        documentBl.RollbackProductTransactions(operationsFromProduct);
                       // BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        response = new BaseOutput()
                        {
                            ResponseCode = SingularHelpers.ErrorCodes.TransactionStatusSuccess
                        };
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        var error = SingularHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                        response = new BaseOutput
                        {
                            ResponseCode = error
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
                        var error = SingularHelpers.GetError(Constants.Errors.GeneralException);
                        response = new BaseOutput
                        {
                            ResponseCode = error
                        };
                    }
                }
            }
            return response;
        }

        [HttpPost]
        [Route("{partnerId}/api/Singular/getExchangeRates")]
        public ExchangeRateOutput[] GetExchangeRates(BaseInput input)
        {
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                try
                {
					if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
					var partner = SingularHelpers.OperatorIds[input.OperatorId];

					CheckSign(input.OperatorId, partnerBl, input.Hash, partner);
                    var response = new ExchangeRateOutput[SingularHelpers.Currencies.Count];
                    BllCurrency currency = null; 
                    for (var i = 0; i < SingularHelpers.Currencies.Count; i++)
                    {
                        currency = CacheManager.GetCurrencyById(SingularHelpers.Currencies[i+2]);
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

        [HttpPost]
        [Route("{partnerId}/api/Singular/exchange")]
        public AmountResponse Exchange(ExchangeInput input)
        {
            AmountResponse response;
            using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                try
                {
					if (!SingularHelpers.OperatorIds.ContainsKey(input.OperatorId))
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperatorId);
					var partner = SingularHelpers.OperatorIds[input.OperatorId];

					var message = string.Format("{0}{1}{2}{3}", input.OperatorId, input.SourceCurrencyId, input.DestinationCurrencyId, 
                        input.Amount);
                    CheckSign(message, partnerBl, input.Hash, partner);

                    decimal convertedAmount = 0m;

                    if (!input.IsReverse)
                    {
                        convertedAmount = ExchangeAmount(input.SourceCurrencyId, input.DestinationCurrencyId, input.Amount);
                    }
                    else
                    {
                        convertedAmount = ExchangeAmount(input.DestinationCurrencyId, input.SourceCurrencyId, input.Amount);
                    }

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

        private void CheckSign(string strParams, PartnerBll partnerBl, string sign, int partnerId)
        {
            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SingularSecretKey);
            if (partnerKey == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
            var ourHash = CommonFunctions.ComputeMd5(strParams + partnerKey);
            if (ourHash != sign)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }

        private SessionIdentity CheckClientSession(string token)
        {
            return ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
        }

        private decimal GetBalanceAmount(DocumentBll documentBl, int clientId, string currency)
        {
            var balance = BaseHelpers.GetClientProductBalance(clientId, 0);
            return (long)(balance * 100) / 100m;
        }

        private Document WriteTransaction(WithdrawInput input, Document betDocument, decimal amount, OperationTypes transactionType)
        {

            var providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).Id;
            var client = CacheManager.GetClientById((int)input.UserId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var product = CacheManager.GetProductByExternalId(providerId, ((int)SingularHelpers.ExternalProductId.BackgammonP2P).ToString());
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
				using (var documentBl = new DocumentBll(clientBl))
				{
					var operationsFromProduct = new ListOfOperationsFromApi
					{
                        CurrencyId = input.CurrencyId,
						//RoundId = additionalData,
						GameProviderId = providerId,
						ProductId = product.Id,
						TransactionId = input.TransactionId,
						OperationItems = new List<OperationItemFromProduct>()
					};
					operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
					{
						Client = client,
						Amount = amount / 100
                    });
                    Document res = null;
                    if (transactionType == OperationTypes.Bet)
                    {
                        res = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                    }
                    else
                    {
                        if (Math.Abs(Math.Abs(amount) - 0) < Constants.Delta)
                            betDocument.State = (int)BetDocumentStates.Lost;
                        else if (amount > 0)
                            betDocument.State = (int)BetDocumentStates.Won;
                        operationsFromProduct.CreditTransactionId = betDocument.Id;
                        res = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl).FirstOrDefault();
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);

                    return res;
                }
            }
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