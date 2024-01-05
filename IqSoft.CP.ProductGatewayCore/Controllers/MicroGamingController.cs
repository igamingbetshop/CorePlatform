/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Interfaces;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.MicroGaming;
using Newtonsoft.Json;
using BaseInput = IqSoft.CP.ProductGateway.Models.MicroGaming.Input;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.BLL.Interfaces;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [XmlDeserializer]
    public class MicroGamingController : ControllerBase
    {
        [HttpPost]
        [Route("{partnerId}/api/MicroGaming/ApiRequest")]
        public ActionResult ApiRequest(int partnerId, BaseInput input)// handle clienttypeid
        {
            ExternalOperation log = null;
            try
            {
                using (var clientBl = new ClientBll(Program.Identity, Program.DbLogger))
                {
                    log = WriteRequestLog(input, clientBl, input.MethodCall.Name);
                    CheckMicroGamingCredentials(clientBl, partnerId, input.MethodCall.Auth.Login,
                        input.MethodCall.Auth.Password);
                    var clientSession = CheckClientSession(input.MethodCall.Call.Token);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    //  check offline parameter
                    var response = new Output();
                    switch (input.MethodCall.Name)
                    {
                        case MicroGamingHelpers.Methods.Login:
                            response = Login(client, response);
                            break;
                        case MicroGamingHelpers.Methods.GetBalance:
                            break;
                        case MicroGamingHelpers.Methods.Play:
                            response = Play(input, clientSession, log, response, clientBl as BaseBll);
                            break;
                        case MicroGamingHelpers.Methods.EndGame:
                            input.MethodCall.Call.PlayType = MicroGamingHelpers.PlayTypes.Win;
                            response = Play(input, clientSession, log, response, clientBl as BaseBll);
                            break;
                        case MicroGamingHelpers.Methods.RefreshToken:
                            break;
                    }
                    if (input.MethodCall.Name != MicroGamingHelpers.Methods.RefreshToken && !response.MethodResponse.Result.AvailableBalance.HasValue)
                    {
                        SetBalance(response, clientBl, client.Id, client.CurrencyId);
                    }
                    var newToken = clientBl.RefreshClientSession(input.MethodCall.Call.Token, input.MethodCall.Name == MicroGamingHelpers.Methods.Login);
                    response.MethodResponse.Result.Token = newToken.Token;
                    return ReturnResponse(response, clientBl, log, input);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
				return ReturnErrorResponse(ex.Detail==null ? Constants.Errors.GeneralException : ex.Detail.Id, log, input, ex);
            }
            catch (Exception ex)
            {
                return ReturnErrorResponse(Constants.Errors.GeneralException, log, input, ex);
            }
        }

        private Output Login(BllClient client, Output response)
        {
            response.MethodResponse.Result.LoginName = client.Id.ToString();
            response.MethodResponse.Result.Currency = client.CurrencyId;
            response.MethodResponse.Result.Country = "ARM";
            response.MethodResponse.Result.City = "ER";
            response.MethodResponse.Result.Wallet = MicroGamingHelpers.Wallets.Vanguard;
            return response;
        }

        private Output Play(Input input, SessionIdentity clientSession, ExternalOperation log, Output response,
            BaseBll baseBl)
        {
            using (var clientDocumentBl = new ClientDocumentBll(baseBl))
            {
                var client = CacheManager.GetClientById(clientSession.Id);
                switch (input.MethodCall.Call.PlayType) // not completed for admin????????
                {
                    case MicroGamingHelpers.PlayTypes.Bet:
                    case MicroGamingHelpers.PlayTypes.TransferToMgs:
                    case MicroGamingHelpers.PlayTypes.TournamentPurchase:
                        return Bet(input, clientSession, log, response, clientDocumentBl);
                    case MicroGamingHelpers.PlayTypes.Win:
                    case MicroGamingHelpers.PlayTypes.ProgressiveWin:
                    case MicroGamingHelpers.PlayTypes.TransferFromMgs:
                        return Win(input, client, log, response, clientDocumentBl);
                    case MicroGamingHelpers.PlayTypes.Refund:
                        return RollBack(input, client, log, response, clientDocumentBl);
                }
                return response;
            }
        }

        private Output Bet(Input input, SessionIdentity clientSession, ExternalOperation log, Output response, IClientDocumentBll clientDocumentBl)
        {
            var client = CacheManager.GetClientById(clientSession.Id);
            var product = CacheManager.GetProductByExternalId(CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id, input.MethodCall.Call.GameReference);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
            if (partnerProductSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerProductSettingNotFound);

            var document = clientDocumentBl.GetDocumentByExternalId(input.MethodCall.Call.ActionId,
                client.Id, CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id, partnerProductSetting.Id, (int)OperationTypes.Bet);

            if (document != null)
            {
                var externalOperation = clientDocumentBl.GetExternalOperationByParentId(document.ExternalOperationId.Value);
                var oldResponse = JsonConvert.DeserializeObject<Output>(externalOperation.Body);
                response.MethodResponse.Result.AvailableBalance = oldResponse.MethodResponse.Result.AvailableBalance;
                response.MethodResponse.Result.BonusBalance = oldResponse.MethodResponse.Result.BonusBalance;
                response.MethodResponse.Result.ExtTransactionId = oldResponse.MethodResponse.Result.ExtTransactionId;
                return response;
            }
            var operationsFromProduct = new ListOfOperationsFromApi
            {
                CurrencyId = input.MethodCall.Call.Currency ?? client.CurrencyId,
                RoundId = input.MethodCall.Call.GameId,
                GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id,
                ExternalOperationId = log.Id,
                ExternalProductId = input.MethodCall.Call.GameReference,
                TransactionId = input.MethodCall.Call.ActionId,
                Info = input.MethodCall.Call.ActionDesc,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
            {
                Client = client,
                Amount = (decimal)input.MethodCall.Call.Amount / 100,
                DeviceTypeId = clientSession.DeviceType
            });
            document = clientDocumentBl.CreateCreditsFromClients(operationsFromProduct).FirstOrDefault();
            response.MethodResponse.Result.ExtTransactionId = document.Id.ToString();
            return response;
        }

        private Output Win(Input input, BllClient client, ExternalOperation log, Output response, IClientBll clientDocumentBl)
        {
            var product = CacheManager.GetProductByExternalId(CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id, input.MethodCall.Call.GameReference);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
            if (partnerProductSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerProductSettingNotFound);

            var document = clientDocumentBl.GetDocumentByExternalId(input.MethodCall.Call.ActionId, client.Id, CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id, 
                partnerProductSetting.Id, (int)OperationTypes.Win);

            if (document != null)
            {
                var externalOperation = clientDocumentBl.GetExternalOperationByParentId(document.ExternalOperationId.Value);
                var oldResponse = JsonConvert.DeserializeObject<Output>(externalOperation.Body);
                response.MethodResponse.Result.AvailableBalance = oldResponse.MethodResponse.Result.AvailableBalance;
                response.MethodResponse.Result.BonusBalance = oldResponse.MethodResponse.Result.BonusBalance;
                response.MethodResponse.Result.ExtTransactionId = oldResponse.MethodResponse.Result.ExtTransactionId;
                return response;
            }
			var betDocument = clientDocumentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.MethodCall.Call.GameId, CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id, client.Id);
			if(betDocument == null)
				throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
			var state = (input.MethodCall.Call.Amount > 0 ? (int)DocumentStates.Won : (int)DocumentStates.Lost);
            betDocument.State = state;

            var operationsFromProduct = new ListOfOperationsFromApi
            {
                CurrencyId = input.MethodCall.Call.Currency ?? client.CurrencyId,
                RoundId = input.MethodCall.Call.GameId,
                GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id,
                ExternalOperationId = log.Id,
                ExternalProductId = input.MethodCall.Call.GameReference,
                TransactionId = input.MethodCall.Call.ActionId,
                CreditTransactionId = betDocument.Id,
                Info = input.MethodCall.Call.ActionDesc,
                State = state,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
            {
                Client = client,
                Amount = (decimal)input.MethodCall.Call.Amount / 100
            });
            document = clientDocumentBl.CreateDebitsToClients(operationsFromProduct, betDocument).FirstOrDefault();
            response.MethodResponse.Result.ExtTransactionId = document.Id.ToString();
            return response;
        }

        private Output RollBack(Input input, BllClient client, ExternalOperation log, Output response, IDocumentBll clientDocumentBl)
        {
            try
            {
                var product = CacheManager.GetProductByExternalId(CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id, input.MethodCall.Call.GameReference);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerProductSettingNotFound);
                var document = clientDocumentBl.GetDocuments(new FilterDocument
                {
                    OperationTypeIds = new List<int> { (int)OperationTypes.BetRollback, (int)OperationTypes.WinRollback, (int)OperationTypes.Rollback },
                    ExternalTransactionId = input.MethodCall.Call.ActionId,
                    GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id
                }).FirstOrDefault();

                if (document != null)
                {
                    var externalOperation = clientDocumentBl.GetExternalOperationByParentId(document.ExternalOperationId.Value);
                    var oldResponse = JsonConvert.DeserializeObject<Output>(externalOperation.Body);
                    response.MethodResponse.Result.AvailableBalance = oldResponse.MethodResponse.Result.AvailableBalance;
                    response.MethodResponse.Result.BonusBalance = oldResponse.MethodResponse.Result.BonusBalance;
                    response.MethodResponse.Result.ExtTransactionId = oldResponse.MethodResponse.Result.ExtTransactionId;
                    return response;
                }
                //check if its bet
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id,
                    TransactionId = input.MethodCall.Call.ActionId,
                    ExternalOperationId = log.Id,
                    Info = input.MethodCall.Call.ActionDesc
                };
                document = clientDocumentBl.RollbackProductTransactions(operationsFromProduct).FirstOrDefault();
                response.MethodResponse.Result.ExtTransactionId = document.Id.ToString();
                return response;
            }
            catch (FaultException<fnErrorType> ex)
            {
                if (ex.Detail.Id == Constants.Errors.DocumentNotFound)
                {
                    response.MethodResponse.Result.ExtTransactionId = "DEBIT-NOT-RECEIVED";
                    return response;
                }
                throw BaseBll.CreateException(string.Empty, ex.Detail.Id);
            }
        }

        private ExternalOperation WriteRequestLog(BaseInput input, IBaseBll bl, string method)
        {
            var request = new ExternalOperation
            {
                Method = method,
                GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id,
                Type = Constants.InternalOperationType.Request,
                Source = Constants.InternalOperationSource.FromGameProvider,
                Body = JsonConvert.SerializeObject(input)
            };
            var log = bl.SaveExternalOperation(request);
            return log;
        }

        private ActionResult ReturnResponse(Output response, IBaseBll bl, ExternalOperation requestLog, Input input)
        {
            var dbDate = DateTime.UtcNow;
            response.MethodResponse.Timestamp = string.Format("{0}/{1}/{2} {3}", dbDate.Year, dbDate.Month < 10 ? "0" + dbDate.Month.ToString() : dbDate.Month.ToString(), 
                dbDate.Day < 10 ? "0" + dbDate.Day.ToString() : dbDate.Day.ToString(), dbDate.ToString("HH:mm:ss.fff"));
            response.MethodResponse.Name = input.MethodCall.Name;
            response.MethodResponse.Result.Seq = input.MethodCall.Call.Seq;
            var request = new ExternalOperation
            {
                Method = input.MethodCall.Name,
                GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id,
                Type = Constants.InternalOperationType.Response,
                Source = Constants.InternalOperationSource.FromGameProvider,
                Body = JsonConvert.SerializeObject(response)
            };
            if (requestLog != null)
                request.ParentId = requestLog.Id;
            bl.SaveExternalOperation(request);
            var output = CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.TextXml,
                CustomXmlFormatter.XmlFormatterTypes.WithoutNamesPacesAndOmitDeclaration);
            return output;
        }

        private ActionResult ReturnErrorResponse(int errorId, ExternalOperation requestLog, Input input, Exception ex = null)
        {
            using (var baseBl = new PartnerBll(Program.Identity, Program.DbLogger))
            {
                if (ex != null)
                    baseBl.WriteErrorLog(ex, requestLog);
                var error = MicroGamingHelpers.GetError(errorId);
                var response = new Output();
                response.MethodResponse.Result.ErrorCode = error.Item1;
                response.MethodResponse.Result.ErrorDescription = error.Item2;
                return ReturnResponse(response, baseBl, requestLog, input);
            }
        }

        private SessionIdentity CheckClientSession(string token)
        {
            return ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
        }

        private void CheckMicroGamingCredentials(BaseBll baseBl, int partnerId, string login, string password)
        {
            var gameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.MicroGaming).Id;
            var partnerMicroGamingLogin = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.PartnerMicroGamingLogin);

            if (partnerMicroGamingLogin == null || partnerMicroGamingLogin == string.Empty)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerKeyNotFound);

            var partnerMicroGamingPassword = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.PartnerMicroGamingPass);
            if (partnerMicroGamingPassword == null || partnerMicroGamingPassword == string.Empty)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerKeyNotFound);
            if (partnerMicroGamingLogin != login || partnerMicroGamingPassword != password)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongApiCredentials);
        }

        private string GetClientRegionIsoCode(List<fnRegion> regions, int type, int clientRegionId, IClientBll clientBl)
        {
            var region = regions.FirstOrDefault(x => x.Id == clientRegionId);
            if (region == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.RegionNotFound);
            if (region.TypeId == type)
                return region.IsoCode;
            if (region.ParentId.HasValue)
                return GetClientRegionIsoCode(regions, type, region.ParentId.Value, clientBl);
            throw BaseBll.CreateException(string.Empty, Constants.Errors.RegionNotFound);
        }

        private Output SetBalance(Output response, IBaseBll baseBll, int clientId, string currencyId)
        {
            var balance = baseBll.GetObjectBalanceWithConvertion((int)ObjectTypes.Client, clientId, currencyId);
            response.MethodResponse.Result.AvailableBalance = (int)(balance.AvailableBalance * 100);
            response.MethodResponse.Result.BonusBalance = "0";
            return response;
        }
    }
}
*/