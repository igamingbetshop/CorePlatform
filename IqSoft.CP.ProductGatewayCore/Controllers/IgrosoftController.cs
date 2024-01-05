using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using IqSoft.CP.ProductGateway.Models.Igrosoft;
using System;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class IgrosoftController : ControllerBase
    {

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "5.9.107.116",
            "213.239.220.134",
            "46.4.30.239",
            "95.216.19.46",
            "195.154.241.32",
            "62.210.180.88",
            "62.210.188.100",
            "62.210.142.135",
            "2a01:4f8:162:4364:0:0:0:2",
            "2a01:4f8:162:4364:0:0:0:2",
            "2a01:4f8:221:348b:0:0:0:2",
            "2a01:4f9:2a:1351:0:0:0:2",
            "2a01:4f8:a0:70cb:0:0:0:2"
        };

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Igrosoft).Id;

        [HttpPost]
        [Route("{partnerId}/api/Igrosoft/ApiRequest")]
        public ActionResult ApiRequest(int partnerId, BaseInput input)
        {
            var jsonResponse = new BaseOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var provider = Request.Headers["X-Casino-Provider-Id"];
                var transId = Request.Headers["X-Casino-Transaction-Id"];
                var timestamp = Request.Headers["X-Casino-Timestamp"];
                var signature = Request.Headers["X-Casino-Signature"];
                var salt = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.IgrosoftSalt);
                if (CommonFunctions.ComputeMd5(provider + transId + timestamp + salt).ToLower() != signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                if (client == null || client.PartnerId != partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                if (input.Withdraw.HasValue && input.Withdraw.Value != 0)
                    jsonResponse.TransactionId = DoBet(input, client);
                if (input.Deposit.HasValue)
                    jsonResponse.TransactionId = DoWin(input, client);
                //bonus ??
                //
                jsonResponse.Status = IgrosoftHelpers.Statuses.Success;
                jsonResponse.SessionId = input.SessionId;
                jsonResponse.Amount = (BaseBll.GetObjectBalance((int)ObjectTypes.Client, Convert.ToInt32(input.ClientId)).AvailableBalance * (input.Denom.HasValue ? input.Denom.Value : 1)).ToString();
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                jsonResponse.Status = IgrosoftHelpers.Statuses.Error;
                jsonResponse.Message = fex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(jsonResponse));
            }
            catch (Exception ex)
            {
                jsonResponse.Status = IgrosoftHelpers.Statuses.Error;
                jsonResponse.Message = ex.Message;
                Program.DbLogger.Error(ex);
            }
            return Ok(jsonResponse);
        }

        private static string DoBet(BaseInput input, BllClient client)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id,
                            ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.GameRound,
                            GameProviderId = ProviderId,
                            ExternalProductId = input.GameId,
                            ProductId = clientSession.ProductId,
                            TransactionId = input.TransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Withdraw.Value / (input.Denom.HasValue ? input.Denom.Value : 1),
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        var lostOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.GameRound,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = input.GameId,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.TransactionId + "_lost",
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Lost,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        lostOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });

                        betDocument = clientBl.CreateDebitsToClients(lostOperationsFromProduct, betDocument, documentBl)[0];
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    return betDocument.Id.ToString();
                }
            }
        }

        private static string DoWin(BaseInput input, BllClient client)
        {
            var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.GameRound, ProviderId, client.Id);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                    var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument == null)
                    {
                        var state = (input.Deposit.Value > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.GameRound,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ExternalProductId = input.GameId,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.TransactionId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Deposit.Value / (input.Denom.HasValue ? input.Denom.Value : 1)
                        });

                        winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    return winDocument.Id.ToString();
                }
            }
        }
    }
}