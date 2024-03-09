using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;
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
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.Common.Helpers;
using System.Globalization;
using IqSoft.CP.Common.Models.Bonus;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.IO;
using System.Web;
using IqSoft.CP.Common.Models.Document;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class IqSoftSiteGamesController : ApiController
    {
        private int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.IqSoft).Id;
        private readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/Authorization")]
        public IHttpActionResult Authorization(int partnerId, AuthorizationInput input)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                    bool isShopWallet = false;
                    var ps = CacheManager.GetClientPlatformSession(client.Id, null);
                    if (ps.AccountId != null)
                    {
                        var accounts = clientBl.GetClientAccounts(client.Id, false);
                        var account = accounts.FirstOrDefault(x => x.Id == ps.AccountId);
                        if (account != null && account.BetShopId != null)
                            isShopWallet = true;
                    }

                    var response = client.MapToAuthorizationOutput(newToken, isShopWallet);
                    response.AvailableBalance = BaseHelpers.GetClientProductBalance(client.Id, productId);
                    if (productId == Constants.SportsbookProductId)
                    {
                        var bonuses = clientBl.GetClientActiveBonuses(client.Id, (int)BonusTypes.CampaignFreeBet, languageId);
                        response.Bonuses = bonuses.Select(x => new ApiBonus 
                        { 
                            Id = x.Id, 
                            BonusId = x.BonusId, 
                            BonusType = x.Type, 
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
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/GetBalance")]
        public IHttpActionResult GetBalance(int partnerId, DAL.Models.Integration.ProductsIntegration.GetBalanceInput input)
        {
            try
            {
                var clientSession = CheckClientSession(input.Token, true);
                using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var currencyId = string.IsNullOrWhiteSpace(input.CurrencyId) ? client.CurrencyId : input.CurrencyId;
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
                    var response = new DAL.Models.Integration.ProductsIntegration.GetBalanceOutput
                    {
                        CurrencyId = client.CurrencyId,
                        AvailableBalance = balance
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
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/Credit")]
        public IHttpActionResult Credit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                var session = CheckClientSession(input.Token, true);
				using (var clientBl = new ClientBll(session, WebApiApplication.DbLogger))
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
                            var document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                            BaseHelpers.BroadcastBetLimit(info);
                            response.OperationItems.Add(new FinOperationOutputItem
                            {
                                BarCode = document.Barcode,
                                BetId = document.Id.ToString(),
                                ClientId = document.ClientId.Value.ToString(),
                                BonusId = document.BonusId,
                                Balance = BaseHelpers.GetClientProductBalance(document.ClientId.Value, session.ProductId),
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
                            WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
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
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/Debit")]
        public IHttpActionResult Debit(int partnerId, ApiFinOperationInput input)
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
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/RollBack")]
        public IHttpActionResult RollBack(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = providerId,
                        ExternalProductId = input.GameId,
                        TransactionId = input.RollbackTransactionId,
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
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input), ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/GetPartnerLanguages")]
        public IHttpActionResult GetPartnerLanguages(int partnerId)
        {
            try
            {
                using (var languageBl = new LanguageBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var partnerLanguages = languageBl.GetPartnerLanguages(partnerId);
                    return Ok(new PartnerLanguagesOutput { Languages = partnerLanguages.Select(x => new ApiLanguage { Id = x.LanguageId, Name = x.Language.Name }).ToList() });
                }
            }
            catch (Exception ex)
            {
                var response = new PartnerLanguagesOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message,
                    Languages = new List<ApiLanguage>()
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/GetFilteredAccounts")]
        public IHttpActionResult GetFilteredAccounts(DAL.Models.Integration.ProductsIntegration.GetBalanceInput input)
        {
            try
            {
                var clientSession = CheckClientSession(input.Token, true);
                var response = new List<long>();
                using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
                {
                    var ps = CacheManager.GetClientPlatformSession(clientSession.Id, null);
                    if (ps.AccountId != null)
                    {
                        var account = clientBl.GetAccount(ps.AccountId.Value);
                        if ((account.TypeId == (int)AccountTypes.ClientUnusedBalance || account.TypeId == (int)AccountTypes.ClientUsedBalance) &&
                            account.BetShopId == null && account.PaymentSystemId == null)
                        {
                            var accounts = clientBl.GetfnAccounts(new FilterfnAccount
                            {
                                ObjectId = ps.ClientId,
                                ObjectTypeId = (int)ObjectTypes.Client
                            });
                            response = accounts.Where(x => (x.TypeId == (int)AccountTypes.ClientUnusedBalance ||
                                x.TypeId == (int)AccountTypes.ClientUsedBalance) && x.BetShopId == null && x.PaymentSystemId == null).Select(x => x.Id).ToList();
                        }
                        else
                            response.Add(ps.AccountId.Value);
                    }

                    return Ok(new Models.IqSoft.ApiResponseBase { ResponseObject = JsonConvert.SerializeObject(response) });
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
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
        }


        [HttpPost]
        [Route("{partnerId}/api/IqSoftSiteGames/AddFreeSpin")]
        public HttpResponseMessage AddFreeRound(int partnerId, ApiFreeSpinInput input)
        {
            var response = new ResponseBase { Description = "Success" };
            try
            {
                var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.IqSoftFreeSpinApiKey);
                var inputToken = CommonFunctions.ComputeMd5($"{partnerId}:{input.ClientId}:{apiKey}");
                if (inputToken != input.ApiToken)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var client = CacheManager.GetClientByUserName(partnerId, Constants.ExternalClientPrefix + input.ClientId.ToString());
                if (client == null || client.PartnerId != partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (!DateTime.TryParseExact(input.ValidUntil, DateTimeFormat, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime validUntil))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.RequestExpired);

                if (input.ProductId == 0 || input.SpinCount <= 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var filter = new FilterfnPartnerProductSetting
                {
                    State = (int)PartnerProductSettingStates.Active,
                    ProductId = input.ProductId
                    //ProductIds = new FiltersOperation
                    //{
                    //    IsAnd = true,
                    //    OperationTypeList = new List<FiltersOperationType>
                    //            {
                    //                new FiltersOperationType
                    //                {
                    //                    OperationTypeId = (int)FilterOperations.InSet,
                    //                    StringValue = string.Join(",", input.ProductIds)
                    //                }
                    //            }
                    //}
                };
                using (var scope = CommonFunctions.CreateTransactionScope())
                {
                    using (var productBl = new ProductBll(new SessionIdentity { LanguageId = Constants.DefaultLanguageId }, WebApiApplication.DbLogger))
                    using (var bonusService = new BonusService(productBl))
                    {
                        var partnerSetting = productBl.GetfnPartnerProductSettings(filter, false).Entities.FirstOrDefault() ??
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                        //var partnerSettingIds = partnerSettings.Select(x => x.Id).ToList();
                        //if (input.ProductIds.Any(x => !partnerSettingIds.Contains(x)))
                        //    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                        //var providerGames = partnerSettings.GroupBy(x => x.GameProviderName).ToDictionary(x => x.Key, x => x.Select(y => y.ProductExternalId).ToList());
                        var product = CacheManager.GetProductById(partnerSetting.ProductId);
                        if(!(product.FreeSpinSupport ?? false))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.UnavailableFreespin);
                        var clientBonus = bonusService.GivetFreeSpinBonus(client.Id, validUntil, input.SpinCount, product.Id);
                        var currentDate = DateTime.UtcNow;
                        var freespinModel = new FreeSpinModel
                        {
                            ClientId = clientBonus.ClientId,
                            BonusId = clientBonus.Id,
                            StartTime = currentDate,
                            FinishTime = validUntil,
                            SpinCount = input.SpinCount,
                            ProductExternalId = partnerSetting.ProductExternalId,
                            Lines = input.Lines,
                            Coins = input.Coins,
                            CoinValue = input.CoinValue,
                            BetValueLevel = input.BetValueLevel
                        };
                        switch (partnerSetting.GameProviderName)
                        {
                            case Constants.GameProviders.TwoWinPower:
                                // create method using productIds
                                //   Integration.Products.Helpers.TwoWinPowerHelpers.SetFreespin(input.ClientId, clientBonus.Id, input.SpinCount, WebApiApplication.DbLogger);
                                break;
                            case Constants.GameProviders.OutcomeBet:
                            case Constants.GameProviders.Mascot:
                                // create method using productIds
                                break;
                            case Constants.GameProviders.BlueOcean:
                                Integration.Products.Helpers.BlueOceanHelpers.AddFreeRound(clientBonus.ClientId, new List<string> { partnerSetting.ProductExternalId },
                                                                                           input.SpinCount, currentDate, validUntil);
                                break;
                            case Constants.GameProviders.SoftGaming:
                                freespinModel.ProductExternalId = partnerSetting.ProductExternalId;
                                freespinModel.SpinCount = input.SpinCount;
                                Integration.Products.Helpers.SoftGamingHelpers.AddFreeRound(freespinModel, WebApiApplication.DbLogger);
                                break;
                            case Constants.GameProviders.PragmaticPlay:
                                Integration.Products.Helpers.PragmaticPlayHelpers.AddFreeRound(clientBonus.ClientId, new List<string> { partnerSetting.ProductExternalId },
                                                                                               input.SpinCount, clientBonus.Id, currentDate, validUntil);
                                break;
                            case Constants.GameProviders.Habanero:
                                Integration.Products.Helpers.HabaneroHelpers.AddFreeRound(clientBonus.ClientId, new List<string> { partnerSetting.ProductExternalId },
                                                                                          input.SpinCount, currentDate, validUntil);
                                break;
                            case Constants.GameProviders.BetSoft:
                                Integration.Products.Helpers.BetSoftHelpers.AddFreeRound(clientBonus.ClientId, new List<string> { partnerSetting.ProductExternalId },
                                                                                         input.SpinCount, clientBonus.Id, currentDate, validUntil);
                                break;
                            case Constants.GameProviders.SoftSwiss:
                                Integration.Products.Helpers.SoftSwissHelpers.AddFreeRound(clientBonus.ClientId, clientBonus.Id, new List<string> { partnerSetting.ProductExternalId },
                                                                                           input.SpinCount, validUntil, WebApiApplication.DbLogger);
                                break;
                            case Constants.GameProviders.EveryMatrix:
                                Integration.Products.Helpers.EveryMatrixHelpers.AwardFreeSpin(freespinModel, WebApiApplication.DbLogger);
                                break;
                            case Constants.GameProviders.PlaynGo:
                                freespinModel.ProductExternalIds = new List<string> { partnerSetting.ProductExternalId };
                                Integration.Products.Helpers.PlaynGoHelpers.AddFreeRound(freespinModel, WebApiApplication.DbLogger);
                                break;
                            case Constants.GameProviders.AleaPlay:
                                Integration.Products.Helpers.AleaPlayHelpers.AddFreeRound(freespinModel, WebApiApplication.DbLogger);                             
                                break;
                            default:
                                break;
                        }
                    }
                    scope.Complete();
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id;
                response.Description =ex.Detail == null ? ex.Message : ex.Detail.Message;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message}  Input: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private IHttpActionResult ProcessDebit(int partnerId, ApiFinOperationInput input)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                    if (operationsFromProduct.State == (int)BetDocumentStates.Cashouted && creditTransaction != null && !string.IsNullOrEmpty(creditTransaction.Info))
                    {
                        try
                        {
                            var info = JsonConvert.DeserializeObject<DocumentInfo>(creditTransaction.Info);
                            if (info != null && info.BonusId > 0)
                                documentBl.RevertClientBonusBet(info, creditTransaction.ClientId.Value, string.Empty, creditTransaction.Amount);
                        }
                        catch 
                        { 
                        
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastWin(new ApiWin
                    {
                        GameName = product.NickName,
                        ClientId = client.Id,
                        ClientName = client.FirstName,
                        Amount = input.Amount,
                        CurrencyId = client.CurrencyId,
                        PartnerId = client.PartnerId,
                        ProductId = product.Id,
                        ProductName = product.NickName,
                        ImageUrl = product.WebImageUrl
                    });
                    foreach (var win in documents)
                    {
                        var outputItem =
                            new FinOperationOutputItem
                            {
                                BarCode = win.Barcode,
                                BetId = win.Id.ToString(),
                                ClientId = win.ClientId.Value.ToString(),
                                Balance = BaseHelpers.GetClientProductBalance(win.ClientId.Value, product.Id)
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
