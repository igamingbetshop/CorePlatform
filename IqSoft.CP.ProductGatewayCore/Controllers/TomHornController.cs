using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.TomHorn;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class TomHornController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.TomHorn).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "94.130.85.104",
            "94.130.86.106",
            "162.55.248.136"
        };
        [HttpPost]
        [Route("{partnerId}/api/TomHorn/GetBalance")]
        public ActionResult GetBalance(BalanceInput input)
        {
            var output = new BalanceOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                //CheckSign(input, client.PartnerId, input.Sign, input.OperatorId, false);
                if (input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                output.Balance = new Balance
                {
                    Amount = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance)),
                    Currency = client.CurrencyId
                };
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                Program.DbLogger.Error(ex);
            }
            var jsonResponse = JsonConvert.SerializeObject(output, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return Ok(jsonResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/TomHorn/Withdraw")]
        public ActionResult DoBet( BetInput input)
        {
            var output = new BetOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameModule);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors. ProductNotFound);
                CheckSign(input, client.PartnerId, input.Sign, product.SubProviderId ?? product.GameProviderId.Value);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        if (input.Currency != client.CurrencyId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        var clientSession = ClientBll.GetClientProductSession(input.SessionId.ToString(), Constants.DefaultLanguageId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id,
                                                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (betDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                RoundId = input.GameRoundId.HasValue ? input.GameRoundId.Value.ToString() : string.Empty,
                                ExternalProductId = input.GameModule,
                                ProductId = product.Id,
                                TransactionId = input.TransactionId.ToString(),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                        }
                        output.Transaction = new Transaction
                        {
                            Id = betDocument.Id,
                            Currency = client.CurrencyId,
                            Balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                Program.DbLogger.Error(ex);
            }
            var jsonResponse = JsonConvert.SerializeObject(output, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return Ok(jsonResponse);
        }

        [HttpPost]
        [Route("{partnerId}/api/TomHorn/Deposit")]
        public ActionResult DoWin(BetInput input)
        {
            var output = new BetOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameModule);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                CheckSign(input, client.PartnerId, input.Sign, product.SubProviderId ?? product.GameProviderId.Value);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        if (input.Currency != client.CurrencyId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        var clientSession = ClientBll.GetClientProductSession(input.SessionId.ToString(), Constants.DefaultLanguageId, null, false);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.GameRoundId.ToString(), ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId.Value.ToString(), client.Id,
                                                           ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.GameRoundId.ToString(),
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalProductId = input.GameModule.ToString(),
                                ProductId = betDocument.ProductId,
                                TransactionId = input.TransactionId.ToString(),
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount

                            });
                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
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
                        }
                        output.Transaction = new Transaction
                        {
                            Id = winDocument.Id,
                            Currency = client.CurrencyId,
                            Balance = decimal.Parse(string.Format("{0:N2}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                Program.DbLogger.Error(ex);
            }
            return Ok(output);
        }

        [HttpPost]
        [Route("{partnerId}/api/TomHorn/RollbackTransaction")]
        public ActionResult RollbackTransaction(RollbackInput input)
        {
            var output = new BetOutput
            {
                Code = TomHornHelpers.ErrorCodes.Success
            };
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var clientSession = ClientBll.GetClientProductSession(input.SessionId.ToString(), Constants.DefaultLanguageId, null, false);
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    CheckSign(input, client.PartnerId, input.Sign, product.SubProviderId ?? product.GameProviderId.Value);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Bet,
                        TransactionId = input.TransactionId.Value.ToString()
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct, false);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = TomHornHelpers.GetError(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id); ;
                output.Message = fex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(output));
            }
            catch (Exception ex)
            {
                output.Code = TomHornHelpers.GetError(Constants.Errors.GeneralException);
                output.Message = ex.Message;
                Program.DbLogger.Error(ex);
            }
            return Ok(output);
        }

        private static void CheckSign(object input, int partnerId, string inputSign, int subProviderId)
        {
            string partnerKey, salt;
            if (subProviderId == ProviderId)
            {
                partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TomHornOperatorId);
                salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TomHornSecretKey);
            }
            else
            {
                partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, subProviderId, Constants.PartnerKeys.TomHorn3OperatorId);
                salt = CacheManager.GetGameProviderValueByKey(partnerId, subProviderId, Constants.PartnerKeys.TomHorn3SecretKey);
            }
            var paramsValues = new List<string>();
            var properties = input.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(input, null);
                if (field.Name.ToLower().Contains("sign") || value == null || string.IsNullOrEmpty(value.ToString()))
                    continue;
                paramsValues.Add(value.ToString());
            }
            var sign = paramsValues.Aggregate(string.Empty, (current, par) => current + par);
            sign = CommonFunctions.ComputeHMACSha256(sign, salt).ToUpperInvariant();
            if (inputSign != sign)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }
    }
}