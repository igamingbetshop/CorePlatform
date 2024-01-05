using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.SkyCity;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Enums;
using System.Text;
using IqSoft.CP.DAL.Models;

using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class SkyCityController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "202.153.191.184",
            "54.199.198.226",
            "210.19.89.66", // skycity
            "103.103.128.109",   // Test IP, Taiwan VPN
            "36.71.90.42",       // Test IP, PureVPN
            "13.228.102.6"      // Test IP, Dev VPN
        };
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SkyCity).Id;

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/balance")]
        public ActionResult GetBalance(int partnerId, BaseInput input)
        {
            var response = new BaseOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));

                Program.DbLogger.Info("GetBalance -> Before call GetClientById(): " + input.UserId);
                var client = CacheManager.GetClientByUserName(partnerId, input.UserId);
                Program.DbLogger.Info(client);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                response = new BaseOutput
                {
                    Result = SkyCityHelpers.ErrorCode.Normal,
                    Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0.##########"))
                };
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            Program.DbLogger.Info(JsonConvert.SerializeObject(response));
            return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
        }

        // roiki Nov9 =================================================
        private void CheckSecureKey(int partnerId, BaseInput input)
        {
            if (!Request.Headers.ContainsKey("key"))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            var inputSign = Request.Headers["key"].ToString();
            var secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SkyCitySecureKey);

            Program.DbLogger.Info("Provider ID : " + ProviderId);
            if (string.IsNullOrEmpty(secureKey))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

            var bodyStream = new StreamReader(Request.Body);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            Program.DbLogger.Info("reqbody: " + bodyText);
            Program.DbLogger.Info(secureKey);

            var jsonMessage = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            Program.DbLogger.Info(jsonMessage);
            //var sign = CommonFunctions.ComputeSha256(jsonMessage+secureKey);
            //var sign = CommonFunctions.ComputeSha256("a"+secureKey);
            var sign = CommonFunctions.ComputeHMACSha256(bodyText, secureKey);

            Program.DbLogger.Info("sign to lower : " + sign.ToLower());
            Program.DbLogger.Info("inputsign.tolower : " + inputSign.ToLower());

            if (sign.ToLower() != inputSign.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/betting")]
        [Route("{partnerId}/api/SkyCity/addbetting")]
        public ActionResult DoBet(int partnerId, BetInput input)
        {
            var response = new BaseOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameCode.ToString());
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                            product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var client = CacheManager.GetClientByUserName(partnerId, input.UserId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var document = documentBl.GetDocumentByExternalId(input.Bet.ExternalTransactionId.ToString(),
                            client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        if (document == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                ExternalProductId = input.GameCode.ToString(),
                                ProductId = partnerProductSetting.ProductId,
                                TransactionId = input.Bet.ExternalTransactionId.ToString(),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Bet.Amount
                            });
                            clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                        }
                        response = new BaseOutput
                        {
                            Result = SkyCityHelpers.ErrorCode.Normal,
                            Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0.##########"))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            Program.DbLogger.Info(JsonConvert.SerializeObject(response));
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/prize")]
        public ActionResult DoWin(int partnerId, BetInput input)
        {
            var response = new BaseOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.GameCode.ToString());
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                            product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var client = CacheManager.GetClientByUserName(partnerId, input.UserId);

                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId,
                            ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = documentBl.GetDocumentByExternalId(input.Bet.ExternalTransactionId.ToString(),
                            client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var state = (input.Bet.WinAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
                                ExternalProductId = input.GameCode.ToString(),
                                ProductId = betDocument.ProductId,
                                TransactionId = input.Bet.ExternalTransactionId.ToString(),
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Bet.WinAmount
                            });
                            clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastBalance(client.Id);
                        }
                        response = new BaseOutput
                        {
                            Result = SkyCityHelpers.ErrorCode.Normal,
                            Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0.##########"))
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            Program.DbLogger.Info(JsonConvert.SerializeObject(response));
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/SkyCity/cancel")]
        public ActionResult Rollback(int partnerId, BetInput input)
        {
            var response = new BaseOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.GameCode.ToString());
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId,
                        product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var client = CacheManager.GetClientByUserName(partnerId, input.UserId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        TransactionId = input.Bet.ExternalTransactionId.ToString(),
                        ProductId = partnerProductSetting.ProductId
                    };
                    var betDocument =
                        documentBl.GetDocumentByExternalId(input.Bet.ExternalTransactionId.ToString(),
                        client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                    {
                        Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                    }
                    if (betDocument.State != (int)BetDocumentStates.Deleted)
                    {
                        documentBl.RollbackProductTransactions(operationsFromProduct);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    response = new BaseOutput
                    {
                        Result = SkyCityHelpers.ErrorCode.Normal,
                        Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0.##########"))
                    };
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(faultException.Detail.Id),
                    Description = faultException.Detail.Message
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = new BaseOutput
                {
                    Result = SkyCityHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    Description = ex.Message
                };
            }
            Program.DbLogger.Info(JsonConvert.SerializeObject(response));
            return Ok(response);
        }
    }
}
