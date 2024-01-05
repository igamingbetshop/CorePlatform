using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using IqSoft.CP.ProductGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.ProductGateway.Models.IqSoft;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class IqSoftSiteGamesController : ControllerBase
    {
        private int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.IqSoft).Id;

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/Authorization")]
        public IActionResult Authorization(int partnerId, AuthorizationInput input)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    BllClient client = null;
                    int productId = Constants.SportsbookProductId;
                    string languageId = Constants.DefaultLanguageId;
                    string newToken = input.Token;
                    try
                    {
                        client = clientBl.ProductsAuthorization(input.Token, out newToken, out productId, out languageId, true);
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail.Id == Constants.Errors.SessionNotFound && input.ProductId != null)
                        {
                            client = clientBl.PlatformAuthorization(input.Token, out SessionIdentity session);
                            var product = CacheManager.GetProductByExternalId(providerId, input.ProductId.Value.ToString());
                            newToken = clientBl.CreateNewProductToken(new SessionIdentity { 
                                Id = client.Id,
                                LanguageId = input.LanguageId,
                                ProductId = product.Id,
                                LoginIp = session.LoginIp,
                                DeviceType = session.DeviceType,
                                ParentId = session.SessionId,
                                SessionId = 0
                            });
                            languageId = session.LanguageId;
                        }
                        else throw;
                    }

                    BaseHelpers.RemoveSessionFromeCache(input.Token, null);
                    var response = client.MapToAuthorizationOutput(newToken);
                    var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client, client.Id, client.CurrencyId);
                    response.AvailableBalance = balance.AvailableBalance;
                    if (productId == Constants.SportsbookProductId)
                    {
                        var bonuses = clientBl.GetClientActiveBonuses(client.Id, (int)BonusTypes.CampaignFreeBet, languageId);
                        response.Bonuses = bonuses.Select(x => new ApiBonus 
                        { 
                            Id = x.Id, 
                            BonusId = x.BonusId, 
                            BonusType = x.BonusType, 
                            Condition = x.Condition,
                            Amount = x.RemainingCredit ?? x.BonusPrize,
                            AllowSplit = x.AllowSplit ?? false
                        }).ToList();
                    }
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/GetBalance")]
        public IActionResult GetBalance(int partnerId, GetBalanceInput input)
        {
            try
            {
                var clientSession = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(clientSession, Program.DbLogger))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var currencyId = string.IsNullOrWhiteSpace(input.CurrencyId) ? client.CurrencyId : input.CurrencyId;
                    var balance = clientBl.GetObjectBalanceWithConvertion((int)ObjectTypes.Client, client.Id, currencyId);
                    var response = new GetBalanceOutput
                    {
                        CurrencyId = balance.CurrencyId,
                        AvailableBalance = balance.AvailableBalance
                    };
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/Credit")]
        public IActionResult Credit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                var session = CheckClientSession(input.Token, true);
				using (var clientBl = new ClientBll(session, Program.DbLogger))
				{
					using (var documentBl = new DocumentBll(clientBl))
					{
						var response = new FinOperationOutput
						{
							OperationItems = new List<FinOperationOutputItem>()
						};
						var typeId = (input.TypeId == null
							? null
							: (int?)IqSoftHelpers.BetTypesMapping.First(x => x.Key == input.TypeId.Value).Value);
						int? deviceType = session.DeviceType;
						var client = CacheManager.GetClientById(session.Id);
                        if (client.CurrencyId != input.CurrencyId)
                            input.Amount = BaseBll.ConvertCurrency(input.CurrencyId, client.CurrencyId, input.Amount);
						var operationsFromProduct = new ListOfOperationsFromApi
						{
                            SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							RoundId = input.RoundId,
							GameProviderId = providerId,
							ExternalOperationId = null,
							ExternalProductId = input.GameId,
							TransactionId = input.TransactionId,
							OperationTypeId = input.OperationTypeId,
							Info = input.Info,
                            TicketInfo = input.Info,
                            TypeId = typeId,
							State = input.BetState,
                            SelectionsCount = input.SelectionsCount,
                            BonusId = input.BonusId,
                            OperationItems = new List<OperationItemFromProduct>
						    {
						    	new OperationItemFromProduct
						    	{
						    		Client = client,
						    		Amount = input.Amount,
						    		DeviceTypeId = deviceType,
						    		Type = input.Type,
						    		PossibleWin = input.PossibleWin
						    	}
						    }
						};

                        try
                        {
                            var document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            response.OperationItems.Add(new FinOperationOutputItem
                            {
                                BarCode = document.Barcode,
                                BetId = document.Id,
                                ClientId = document.ClientId.Value,
                                BonusId = document.BonusId,
                                Balance = CacheManager.GetClientCurrentBalance(document.ClientId.Value).AvailableBalance,
                            });
                        }
                        catch (FaultException<BllFnErrorType> ex)
                        {
                            if (ex.Detail.Id != Constants.Errors.ClientMaxLimitExceeded &&
                                ex.Detail.Id != Constants.Errors.PartnerProductLimitExceeded)
                                throw;
                            response.ResponseCode = ex.Detail.Id;
                            response.OperationItems.Add(new FinOperationOutputItem
                            {
                                CurrentLimit = ex.Detail.DecimalInfo == null ? 0 : ex.Detail.DecimalInfo.Value
                            });
                            Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
                        }
						return Ok(response);
					}
				}
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/Debit")]
        public IActionResult Debit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                return ProcessDebit(partnerId, input);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + JsonConvert.SerializeObject(response));
                
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/RollBack")]
        public IActionResult RollBack(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var clientSession = CheckClientSession(input.Token, false);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = providerId,
                        ExternalProductId = input.GameId,
                        TransactionId = input.TransactionId,
                        ExternalOperationId = null,
                        Info = input.Info,
                        OperationTypeId = input.OperationTypeId
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(input.ClientId);
                    return Ok(new ResponseBase());
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input), ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/GetPartnerLanguages")]
        public IActionResult GetPartnerLanguages(int partnerId)
        {
            try
            {
                using (var languageBl = new LanguageBll(new SessionIdentity(), Program.DbLogger))
                {
                    var partnerLanguages = languageBl.GetPartnerLanguages(partnerId);
                    return Ok(new PartnerLanguagesOutput { Languages = partnerLanguages.Select(x => new ApiLanguage { Id = x.LanguageId, Name = x.Language.Name }).ToList() });
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new PartnerLanguagesOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message,
                    Languages = new List<ApiLanguage>()
                };
                return Ok(response);
            }
        }

        private IActionResult ProcessDebit(int partnerId, ApiFinOperationInput input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var response = new FinOperationOutput
                    {
                        OperationItems = new List<FinOperationOutputItem>()
                    };
                    BllClient client = null;
                    if (input.ClientId > 0)
                        client = CacheManager.GetClientById(input.ClientId);
                    else
                        client = CacheManager.GetClientByUserName(partnerId, input.UserName);
                    
                    if (client.CurrencyId != input.CurrencyId)
                        input.Amount = BaseBll.ConvertCurrency(input.CurrencyId, client.CurrencyId, input.Amount);
                    var gameProviderId = providerId;
                    var product = CacheManager.GetProductByExternalId(gameProviderId, input.GameId);

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
                        product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw new ArgumentNullException(Constants.Errors.ProductNotFound.ToString());
                    Document creditTransaction = null;
                    if (input.OperationTypeId != (int)OperationTypes.WageringBonus)
                    {
                        creditTransaction = documentBl.GetDocumentByExternalId(input.CreditTransactionId,
                            client.Id, gameProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (creditTransaction == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                        if (input.BetState != null)
                            creditTransaction.State = input.BetState.Value;
                    }
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        CurrencyId = client.CurrencyId,
                        RoundId = input.RoundId,
                        GameProviderId = providerId,
                        OperationTypeId = input.OperationTypeId,
                        ExternalOperationId = null,
                        ExternalProductId = input.GameId,
                        TransactionId = input.TransactionId,
                        CreditTransactionId = (creditTransaction == null ? (long?)null : creditTransaction.Id),
                        Info = input.Info,
                        State = input.BetState,
                        TicketInfo = input.Info,
                        IsFreeBet = input.IsFreeBet,
                        OperationItems = new List<OperationItemFromProduct>
                        {
                            new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount
                            }
                        }
                    };
                    if (input.OperationTypeId == (int)OperationTypes.CashOut)
                    {
                        operationsFromProduct.State = (int)BetDocumentStates.Cashouted;
                        if (creditTransaction != null)
                            creditTransaction.State = (int)BetDocumentStates.Cashouted;
                    }

                    var documents = clientBl.CreateDebitsToClients(operationsFromProduct, creditTransaction, documentBl);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                    foreach (var win in documents)
                    {
                        var outputItem =
                            new FinOperationOutputItem
                            {
                                BarCode = win.Barcode,
                                BetId = win.Id,
                                ClientId = win.ClientId.Value,
                                Balance = CacheManager.GetClientCurrentBalance(win.ClientId.Value).AvailableBalance
                            };
                        response.OperationItems.Add(outputItem);
                    }
                    return Ok(response);
                }
            }
        }

        private SessionIdentity CheckClientSession(string token, bool checkExpiration)
        {
            var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, null, checkExpiration);
            return session;
        }
    }
}
