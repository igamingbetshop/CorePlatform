using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.YSB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class YSBController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.YSB).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.YSB);
        private static readonly BllProduct product = CacheManager.GetProductByExternalId(ProviderId, "YSB");
        private readonly int Ratio = 1;

        [HttpPost]
        [Route("{partnerId}/api/YSB/ValidateLogin")]
        public HttpResponseMessage ValidateTicket(int partnerId)
        {
            var response = new ValidationOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var deserializer = new XmlSerializer(typeof(ValidationInput));
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var input = (ValidationInput)deserializer.Deserialize(bodyStream);

                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                var clientId = input.Elem.Properties.FirstOrDefault(x => x.Name == "UN").Value;
                var token = input.Elem.Properties.FirstOrDefault(x => x.Name == "SG").Value;

                response.Action = input.Action;
                response.Elem.Id = input.Elem.Id;
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                response.Elem.Properties.Add(new Property { Name = "UN", Value = clientId });
                response.Elem.Properties.Add(new Property { Name = "UID", Value = client.Id.ToString() });
                response.Elem.Properties.Add(new Property { Name = "S", Value = "0" });
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);

            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                WebApiApplication.DbLogger.Error(faultException.Detail.Message);
                response.Elem.Properties.Add(new Property { Name = "ED", Value = faultException.Detail.Message });
                response.Elem.Properties.Add(new Property { Name = "S", Value = YSBHelpers.GetErrorCode(faultException.Detail.Id).ToString() });
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);

            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.Elem.Properties.Add(new Property { Name = "ED", Value = ex.Message });
                response.Elem.Properties.Add(new Property { Name = "S", Value = YSBHelpers.ErrorCodes.GENERAL.ToString() });
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/YSB")]
        public HttpResponseMessage ApiRequest(int partnerId)
        {
            var response = new ValidationOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var deserializer = new XmlSerializer(typeof(ValidationInput));
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var input = (ValidationInput)deserializer.Deserialize(bodyStream);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                var action = input.Action;
                switch (action)
                {
                    case YSBHelpers.Actions.GetBalance:
                        GetBalance(input, out response);
                        break;
                    case YSBHelpers.Actions.Bet:
                        DoBet(input, out response);
                        break;
                    case YSBHelpers.Actions.BetConfirmation:
                        BetConfirmation(input, out response);
                        break;
                    case YSBHelpers.Actions.Payout:
                        DoWin(input, out response);
                        break;
                }
                WebApiApplication.DbLogger.Info("Output: " + JsonConvert.SerializeObject(response));
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                WebApiApplication.DbLogger.Error(faultException.Detail.Message);
                response.Elem.Properties.Add(new Property { Name = "ED", Value = faultException.Detail.Message });
                response.Elem.Properties.Add(new Property { Name = "S", Value = YSBHelpers.GetErrorCode(faultException.Detail.Id).ToString() });
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.Elem.Properties.Add(new Property { Name = "ED", Value = ex.Message });
                response.Elem.Properties.Add(new Property { Name = "S", Value = YSBHelpers.ErrorCodes.GENERAL.ToString() });
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        private void GetBalance(ValidationInput input, out ValidationOutput output)
        {
            output = new ValidationOutput();
            var clientInfo = input.Elem.Properties.FirstOrDefault(x => x.Name == "UN").Value;
            var sign = input.Elem.Properties.FirstOrDefault(x => x.Name == "HP").Value;
            var ind = clientInfo.IndexOf("_");
            var clientId = Convert.ToInt32(clientInfo.Substring(ind + 1, clientInfo.Length - ind - 1));
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.YSBSecretKey + client.CurrencyId);

            var signature = string.Format("{0}|{1}|{2}", YSBHelpers.Actions.GetBalance, clientInfo, secretKey);
            signature = CommonFunctions.ComputeMd5(signature);
            if (signature.ToLower() != sign.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var currency = client.CurrencyId == "CNY" ? "RMB" : client.CurrencyId;
                var balance = (BaseHelpers.GetClientProductBalance(clientId, 0) / Ratio).ToString("0.##");
                output.Action = input.Action;
                output.Elem.Id = input.Elem.Id;
                output.Elem.Properties.Add(new Property { Name = "UN", Value = clientInfo });
                output.Elem.Properties.Add(new Property { Name = "CC", Value = currency });
                output.Elem.Properties.Add(new Property { Name = "BAL", Value = balance });
                output.Elem.Properties.Add(new Property { Name = "S", Value = "0" });
                signature = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", YSBHelpers.Actions.GetBalance,
                    clientInfo, currency,
                    balance, 0,
                    secretKey);
                output.Elem.Properties.Add(new Property { Name = "HP", Value = CommonFunctions.ComputeMd5(signature) });
            }
        }

        void DoBet(ValidationInput input, out ValidationOutput output)
        {
            output = new ValidationOutput();
            var deserializer = new XmlSerializer(typeof(ValidationInput));
            var clientInfo = input.Elem.Properties.FirstOrDefault(x => x.Name == "UN").Value;
            var ind = clientInfo.IndexOf("_");
            var clientId = Convert.ToInt32(clientInfo.Substring(ind + 1, clientInfo.Length - ind - 1));
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var vendor = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.YSBVendor + client.CurrencyId);
            var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.YSBSecretKey + client.CurrencyId);
            var sign = input.Elem.Properties.FirstOrDefault(x => x.Name == "HP").Value;
            var externalTransactionId = input.Elem.Properties.FirstOrDefault(x => x.Name == "TRX").Value;
            var currency = input.Elem.Properties.FirstOrDefault(x => x.Name == "CC").Value;
            var amount = Convert.ToDecimal(input.Elem.Properties.FirstOrDefault(x => x.Name == "AMT").Value);
            var signature = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", YSBHelpers.Actions.Bet,
                externalTransactionId, clientInfo,
                vendor, currency, amount,
                secretKey);
            signature = CommonFunctions.ComputeMd5(signature);
            if (signature.ToLower() != sign.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    var document = documentBl.GetDocumentByExternalId(externalTransactionId, client.Id,
                        ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                    if (document != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.TransactionAlreadyExists);

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ProductId = product.Id,
                        TransactionId = externalTransactionId,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = amount
                    });
                    clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBetLimit(info);
                    var balance = (BaseHelpers.GetClientProductBalance(clientId, product.Id) / Ratio).ToString("0.##");
                    output.Action = input.Action;
                    output.Elem.Id = input.Elem.Id;
                    output.Elem.Properties.Add(new Property { Name = "UN", Value = clientInfo });
                    output.Elem.Properties.Add(new Property { Name = "CC", Value = client.CurrencyId == "CNY" ? "RMB" : client.CurrencyId });
                    output.Elem.Properties.Add(new Property { Name = "BAL", Value = balance });
                    output.Elem.Properties.Add(new Property { Name = "TRX", Value = externalTransactionId });
                    output.Elem.Properties.Add(new Property { Name = "VID", Value = vendor });
                    output.Elem.Properties.Add(new Property { Name = "S", Value = "0" });


                    signature = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", YSBHelpers.Actions.Bet,
                                               externalTransactionId, clientInfo,
                                               vendor, currency, balance,
                                               0, secretKey);
                    output.Elem.Properties.Add(new Property { Name = "HP", Value = CommonFunctions.ComputeMd5(signature) });
                }
            }
        }

        private void BetConfirmation(ValidationInput input, out ValidationOutput output)
        {
            output = new ValidationOutput();
            var clientInfo = input.Elem.Properties.FirstOrDefault(x => x.Name == "UN").Value;
            var ind = clientInfo.IndexOf("_");
            var clientId = Convert.ToInt32(clientInfo.Substring(ind + 1, clientInfo.Length - ind - 1));
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.YSBSecretKey + client.CurrencyId);
            var sign = input.Elem.Properties.FirstOrDefault(x => x.Name == "HP").Value;
            var externalTransactionId = input.Elem.Properties.FirstOrDefault(x => x.Name == "TRX").Value;
            var currency = input.Elem.Properties.FirstOrDefault(x => x.Name == "CC").Value;
            var betStatus = input.Elem.Properties.FirstOrDefault(x => x.Name == "BETSTS").Value;
            var totalRecord = input.Elem.Properties.FirstOrDefault(x => x.Name == "REC").Value;
            var totalBetAmount = Convert.ToDecimal(input.Elem.Properties.FirstOrDefault(x => x.Name == "TOTAL").Value);
            var refundAmount = Convert.ToDecimal(input.Elem.Properties.FirstOrDefault(x => x.Name == "REFUND").Value);


            var signature = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", YSBHelpers.Actions.BetConfirmation,
                externalTransactionId, clientInfo,
                currency, betStatus, totalRecord,
                totalBetAmount, refundAmount,
                secretKey);
            signature = CommonFunctions.ComputeMd5(signature);
            if (signature.ToLower() != sign.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    var document = documentBl.GetDocumentByExternalId(externalTransactionId, client.Id,
                        ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);

                    documentBl.UpdateDocumentExternalId(document.Id, document.ExternalTransactionId,
                                                               JsonConvert.SerializeObject(input.Elem.Records));
                    var pretendWinDocument = documentBl.GetDocumentByExternalId(externalTransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    document.State = (int)BetDocumentStates.Lost;
                    if (pretendWinDocument == null)
                    {
                        var recOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = product.Id,
                            TransactionId = externalTransactionId,
                            CreditTransactionId = document.Id,
                            State = (int)BetDocumentStates.Lost,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        recOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });

                        clientBl.CreateDebitsToClients(recOperationsFromProduct, document, documentBl);
                    }
                    foreach (var record in input.Elem.Records)
                    {
                        var recExternalTransactionId = record.Properties.FirstOrDefault(x => x.Name == "REFID").Value;
                        var recDocument = documentBl.GetDocumentByExternalId(recExternalTransactionId, client.Id,
                        ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                        if (recDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = recExternalTransactionId,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                    }
                    var winDocument = documentBl.GetDocumentByExternalId(externalTransactionId,
                        client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = product.Id,
                            TransactionId = externalTransactionId,
                            CreditTransactionId = document.Id,
                            State = (int)BetDocumentStates.Lost,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });

                        clientBl.CreateDebitsToClients(operationsFromProduct, document, documentBl);
                    }
                    var balance = (BaseHelpers.GetClientProductBalance(clientId, product.Id) / Ratio).ToString();
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    output.Action = input.Action;
                    output.Elem.Id = input.Elem.Id;
                    output.Elem.Properties.Add(new Property { Name = "UN", Value = clientInfo });
                    output.Elem.Properties.Add(new Property { Name = "CC", Value = client.CurrencyId == "CNY" ? "RMB" : client.CurrencyId });
                    output.Elem.Properties.Add(new Property { Name = "BAL", Value = balance });
                    output.Elem.Properties.Add(new Property { Name = "TRX", Value = externalTransactionId });
                    output.Elem.Properties.Add(new Property { Name = "S", Value = "0" });

                    signature = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", YSBHelpers.Actions.BetConfirmation,
                                               externalTransactionId, clientInfo,
                                               currency, balance,
                                               0, secretKey);
                    output.Elem.Properties.Add(new Property { Name = "HP", Value = CommonFunctions.ComputeMd5(signature) });
                }
            }
        }

        private void DoWin(ValidationInput input, out ValidationOutput output)
        {
            output = new ValidationOutput();
            var clientInfo = input.Elem.Properties.FirstOrDefault(x => x.Name == "UN").Value;
            var ind = clientInfo.IndexOf("_");
            var clientId = Convert.ToInt32(clientInfo.Substring(ind + 1, clientInfo.Length - ind - 1));
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.YSBSecretKey + client.CurrencyId);
            var vendor = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.YSBVendor + client.CurrencyId);
            var sign = input.Elem.Properties.FirstOrDefault(x => x.Name == "HP").Value;
            var externalTransactionId = input.Elem.Properties.FirstOrDefault(x => x.Name == "TRX").Value;
            var currency = input.Elem.Properties.FirstOrDefault(x => x.Name == "CC").Value;
            var amount = Convert.ToDecimal(input.Elem.Properties.FirstOrDefault(x => x.Name == "PAYAMT").Value);
            var betStatus = input.Elem.Properties.FirstOrDefault(x => x.Name == "BETSTS").Value;
            var betTransactionId = input.Elem.Properties.FirstOrDefault(x => x.Name == "REFID").Value;
            var betType = input.Elem.Properties.FirstOrDefault(x => x.Name == "BETTYPE").Value;
            var paymentTime = input.Elem.Properties.FirstOrDefault(x => x.Name == "PAYTIME").Value;
            var resettlement = input.Elem.Properties.FirstOrDefault(x => x.Name == "RESETTLEMENT").Value;
            var cashoutId = input.Elem.Properties.FirstOrDefault(x => x.Name == "CASHOUTID").Value;
            var resetImplement = Convert.ToInt32(input.Elem.Properties.FirstOrDefault(x => x.Name == "RESETTLEMENT").Value);
            var signature = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}", YSBHelpers.Actions.Payout,
                externalTransactionId, betType,
                betTransactionId, clientInfo, vendor,
                currency, amount, paymentTime,
                betStatus, resettlement, cashoutId,
                secretKey);
            signature = CommonFunctions.ComputeMd5(signature);
            if (signature.ToLower() != sign.ToLower())
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    var betDocument = documentBl.GetDocumentByExternalId(betTransactionId, client.Id,
                        ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);

                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                    var winDocument = documentBl.GetDocumentByExternalId(externalTransactionId,
                        client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                    if (winDocument == null)
                    {
                        if (resetImplement == 1)
                        {
                            var previousWins = documentBl.GetDocumentsByParentId(betDocument.Id);
                            foreach (var w in previousWins)
                            {
                                if (w.State != (int)BetDocumentStates.Deleted)
                                {
                                    var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        GameProviderId = ProviderId,
                                        TransactionId = w.ExternalTransactionId,
                                        ProductId = product.Id
                                    };

                                    documentBl.RollbackProductTransactions(rollbackOperationsFromProduct);
                                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                }
                            }
                        }
                        var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = product.Id,
                            TransactionId = externalTransactionId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = amount
                        });

                        clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    var balance = (BaseHelpers.GetClientProductBalance(clientId, product.Id) / Ratio).ToString("0.##");
                    output.Action = input.Action;
                    output.Elem.Id = input.Elem.Id;
                    output.Elem.Properties.Add(new Property { Name = "UN", Value = clientInfo });
                    output.Elem.Properties.Add(new Property { Name = "CC", Value = client.CurrencyId == "CNY" ? "RMB" : client.CurrencyId });
                    output.Elem.Properties.Add(new Property { Name = "BAL", Value = balance });
                    output.Elem.Properties.Add(new Property { Name = "TRX", Value = externalTransactionId });
                    output.Elem.Properties.Add(new Property { Name = "S", Value = "0" });
                    signature = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", YSBHelpers.Actions.Payout,
                                               externalTransactionId, clientInfo,
                                               currency, balance,
                                               0, secretKey);
                    output.Elem.Properties.Add(new Property { Name = "HP", Value = CommonFunctions.ComputeMd5(signature) });
                }
            }
        }
    }
}
