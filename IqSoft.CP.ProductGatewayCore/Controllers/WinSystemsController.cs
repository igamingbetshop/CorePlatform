using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.WinSystems;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using Microsoft.Extensions.Primitives;
using System.Linq;
using IqSoft.CP.DAL.Models.Notification;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class WinSystemsController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.WinSystems).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "10.1.12.197",
            "10.1.12.192"
        };
        [HttpPost]
        [Route("{partnerId}/api/WinSystems/GetPlayerInfo")]
        public ActionResult GetPlayerInfo(PlayerInfoInput input, [FromRoute] int partnerId)
        {
            var response = new ApiResponseBase();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.WinSystemAPIKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}", partnerId, input.PlayerId, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (!int.TryParse(input.PlayerId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var client = CacheManager.GetClientById(clientId);
                if (client == null || client.PartnerId != partnerId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var clientIdentities = clientBl.GetClientIdentities(client.Id)
                                                  .OrderByDescending(x => x.Status == (int)KYCDocumentStates.Approved)
                                                  .ThenByDescending(x => x.Status ==(int)KYCDocumentStates.InProcess)
                                                  .ThenByDescending(x => x.Status);
                    var utilityBill = clientIdentities.FirstOrDefault(x => x.DocumentTypeId == (int)KYCDocumentTypes.UtilityBill);
                    var idCard = clientIdentities.FirstOrDefault(x => x.DocumentTypeId == (int)KYCDocumentTypes.IDCard);
                    response.ResponseObject = new
                    {
                        PlayerId = client.Id.ToString(),
                        PlayerCurrency = client.CurrencyId,
                        AvailableBalance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2),
                        client.MobileNumber,
                        client.Email,
                        AddressStatus = utilityBill== null ? 0 : utilityBill.Status,
                        OMANGStatus = idCard== null ? 0 : idCard.Status,
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/WinSystems/GetClient")]
        public ActionResult GetClient(ClientInput input, [FromRoute] int partnerId)
        {
            var response = new ApiResponseBase();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.WinSystemAPIKey);
                var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}", partnerId, input.MobileNumber, partnerKey));
                if (signature.ToLower() != input.Signature)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var client = CacheManager.GetClientByMobileNumber(partnerId, input.MobileNumber);
                if (client == null)
                {
                    if (input.MobileNumber.Contains("+"))
                        input.MobileNumber = input.MobileNumber.Replace("+", string.Empty);
                    else
                        input.MobileNumber = "+" + input.MobileNumber;
                    client = CacheManager.GetClientByMobileNumber(partnerId, input.MobileNumber);
                    if (client != null)
                    {
                        response.ResponseCode = 0;
                        response.ResponseObject = true;
                    }
                    else
                    {
                        response.ResponseCode = Constants.Errors.ClientNotFound;
                        response.ResponseObject = false;
                    }
                }
                else
                {
                    response.ResponseCode = 0;
                    response.ResponseObject = true;
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/WinSystems/Authorization")]
        [Route("{partnerId}/api/WinSystems/GetBalance")]
        public ActionResult Authorization(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            Program.DbLogger.Info("Input: " + inputString);
            var response = new AuthorizationOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var input = JsonConvert.DeserializeObject<BaseInput>(inputString);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.ExternalId != input.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                response.ClientId = client.Id.ToString();
                response.CurrencyId = client.CurrencyId;
                response.Balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + inputString + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + inputString + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/WinSystems/Credit")]
        public ActionResult DoBet(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            Program.DbLogger.Info("Input: " + inputString);
            var response = new TransactionOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var input = JsonConvert.DeserializeObject<TransactionInput>(inputString);
                var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.ExternalId != input.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (client.CurrencyId != input.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (input.Amount <= 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                       
                        var betDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (betDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = input.TransactionId,
                            RoundId = input.RoundId,
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
                        response.ClientId = client.Id.ToString();
                        response.CurrencyId = client.CurrencyId;
                        response.Balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
                        response.BetId = betDocument.Id.ToString();
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + inputString + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + inputString + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/WinSystems/Debit")]
        public ActionResult DoWin(TransactionInput input, [FromRoute]int partnerId)
        {
            var response = new TransactionOutput();
            try
            {
                BllClient client;
                BllProduct product;
                BllPartnerProductSetting partnerProductSetting;
                if (!string.IsNullOrEmpty(input.Token))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                    client = CacheManager.GetClientById(clientSession.Id);
                    product = CacheManager.GetProductById(clientSession.ProductId);
                    if (product.ExternalId != input.ProductId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (client.CurrencyId != input.CurrencyId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                }
                else
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                    product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                }
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (input.Amount < 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var betDocument = documentBl.GetDocumentByExternalId(input.CreditTransactionId, ProviderId, product.Id, OperationTypes.Bet, null);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        client = CacheManager.GetClientById(betDocument.ClientId.Value);
                        if (client.CurrencyId != input.CurrencyId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                             partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                        var state = input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        betDocument.State = state;

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            TransactionId = input.TransactionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = betDocument.ProductId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount
                        });
                        var winDocuments = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);

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
                        response.TransactionId = winDocuments[0].Id.ToString();
                        response.ClientId = client.Id.ToString();
                        response.CurrencyId = client.CurrencyId;
                        response.Balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
                        response.BetId = betDocument.Id.ToString();
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " +  JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " +  JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/WinSystems/Rollback")]
        public ActionResult Rollback(TransactionInput input)
        {
            var response = new TransactionOutput();
            try
            {
                if (string.IsNullOrEmpty(input.Token))
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                }
                else
                    ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: false);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        ProductId = product.Id,
                        TransactionId = input.RollbackTransactionId
                    };
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(doc[0].ClientId.Value);
                    var client = CacheManager.GetClientById(doc[0].ClientId.Value);
                    response.ClientId = client.Id.ToString();
                    response.CurrencyId = client.CurrencyId;
                    response.Balance = Math.Round(CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance, 2);
                    response.TransactionId = doc[0].Id.ToString();
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return Ok(response);
        }

        [Route("{partnerId}/api/WinSystems/SMS")]
        public ActionResult SendSMS(NotificationInput input, [FromRoute] int partnerId)
        {
            var responseBase = new ApiResponseBase { ResponseCode = 0, Description = "Success" };
            try
            {
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                SendNotification(partnerId, ClientMessageTypes.Sms, input);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                responseBase.ResponseCode = fex.Detail.Id;
                responseBase.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                responseBase.ResponseCode = Constants.Errors.GeneralException; ;
                responseBase.Description = ex.Message;
            }
            return Ok(responseBase);
        }


        [Route("{partnerId}/api/WinSystems/Email")]
        public ActionResult SendEmail(NotificationInput input, [FromRoute] int partnerId)
        {
            var responseBase = new ApiResponseBase { ResponseCode = 0, Description = "Success" };
            try
            {
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                SendNotification(partnerId, ClientMessageTypes.Email, input);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                responseBase.ResponseCode = fex.Detail.Id;
                responseBase.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                responseBase.ResponseCode = Constants.Errors.GeneralException; ;
                responseBase.Description = ex.Message;
            }
            return Ok(responseBase);
        }


        [Route("{partnerId}/api/WinSystems/Notification")]
        public ActionResult SendTicket(NotificationInput input, [FromRoute] int partnerId)
        {
            var responseBase = new ApiResponseBase { ResponseCode = 0, Description = "Success" };
            try
            {
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                SendNotification(partnerId, ClientMessageTypes.MessageFromSystem, input);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                responseBase.ResponseCode = fex.Detail.Id;
                responseBase.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                responseBase.ResponseCode = Constants.Errors.GeneralException; ;
                responseBase.Description = ex.Message;
            }
            return Ok(responseBase);
        }

        private void SendNotification(int partnerId, ClientMessageTypes type, NotificationInput input)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.WinSystemAPIKey);
            var signature = CommonFunctions.ComputeMd5(string.Format("{0}|{1}|{2}", partnerId, input.ClientId, partnerKey));
            if (signature.ToLower() != input.Signature)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            if (!int.TryParse(input.ClientId, out int clientId))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var client = CacheManager.GetClientById(clientId);
            if (client == null || client.PartnerId != partnerId)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            if (input.Type.HasValue && !Enum.IsDefined(typeof(ClientInfoTypes), input.Type))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
            using (var notificationBll = new NotificationBll(new SessionIdentity(), Program.DbLogger))
            {
                if (type == ClientMessageTypes.Email || type == ClientMessageTypes.Sms)
                    notificationBll.SendNotificationMessage(new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                        MobileOrEmail = type == ClientMessageTypes.Email ? client.Email : client.MobileNumber,
                        ClientInfoType = input.Type,
                        MessageText = input.Message,
                        MessageType = (int)type
                    });
                else
                    notificationBll.SendInternalTicket(client.Id, input.Type, input.Message);
            }
        }
    }
}