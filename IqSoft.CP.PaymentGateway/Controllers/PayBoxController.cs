using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PayBox;
using System;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PayBoxController : ApiController
    {
        [HttpPost]
        [Route("api/Paybox/Check")]
        public HttpResponseMessage Check(ResultInput input)
        {
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.pg_order_id));
                    if (request == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    
                    var client = CacheManager.GetClientById(request.ClientId.Value);
                    var sign = input.pg_sig;
                    input.pg_sig = null;
                    var script = "Check";                  

                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBoxATM);
                    if (paymentSystem == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var secKey = partnerPaymentSetting.Password;
                    string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(input, ";"), secKey);
                    input.pg_sig = CommonFunctions.ComputeMd5(signature);

                    if (input.pg_sig != sign)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);


                    var response = new CheckOutput
                    {
                        Status = PayBoxHelpers.Statuses.Ok,
                        Salt = CommonFunctions.GetRandomString(16)
                    };
                    signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(response, ";"), secKey);
                    response.Signature = CommonFunctions.ComputeMd5(signature);
                    return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var error = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error,
                    ErrorCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    ErrorDescription = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(new Exception(JsonConvert.SerializeObject(error)));
                return CommonFunctions.ConvertObjectToXml(error, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error,
                    ErrorCode = PayBoxHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorDescription = ex.Message
                };
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        [HttpPost]
        [Route("api/Paybox/Refund")]
        public HttpResponseMessage Refund(RefundInput input)
        {
            try
            {
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.pg_order_id));
                    if (request == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

					var client = CacheManager.GetClientById(request.ClientId.Value);
					var sign = input.pg_sig;
					input.pg_sig = null;
					var script = "Refund";
					var secKey = GetPaymentSetting(client.PartnerId, input.pg_currency, string.Empty).Password;
					string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(input, ";"), secKey);
					input.pg_sig = CommonFunctions.ComputeMd5(signature);

					if (input.pg_sig != sign)
						throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);

					request.Status = (int)PaymentRequestStates.Deleted;
					paymentSystemBl.ChangePaymentRequestDetails(request);
					var response = new CheckOutput
					{
						Status = PayBoxHelpers.Statuses.Ok,
						Salt = CommonFunctions.GetRandomString(16)
					};
					signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(response, ";"), secKey);
					response.Signature = CommonFunctions.ComputeMd5(signature);
					return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
				}
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var error = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error,
                    ErrorCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    ErrorDescription = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(new Exception(JsonConvert.SerializeObject(error)));
                return CommonFunctions.ConvertObjectToXml(error, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error,
                    ErrorCode = PayBoxHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorDescription = ex.Message
                };
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        [HttpPost]
        [Route("api/Paybox/Result")]
        public HttpResponseMessage Result(ResultInput input)
        {
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.pg_order_id));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        var sign = input.pg_sig;
                        input.pg_sig = null;
                        var script = "Result";
                        var partnerPaymentSetting = GetPaymentSetting(client.PartnerId, input.pg_currency, input.pg_payment_system);
                        var secKey = partnerPaymentSetting.Password;
                        string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(input, ";"), secKey);
                        input.pg_sig = CommonFunctions.ComputeMd5(signature);
                        if (input.pg_sig != sign)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);

                        if (request.Amount != Convert.ToDecimal(input.pg_amount))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                        if (request.CurrencyId != input.pg_ps_currency)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);

                        if (Convert.ToInt32(input.pg_result) == 1)//success
                        {
                            request.ExternalTransactionId = input.pg_payment_id;
                            request.PaymentSystemId = partnerPaymentSetting.PaymentSystemId;
                            request.PartnerPaymentSettingId = partnerPaymentSetting.Id;
                            if (input.pg_card_id != null)
                            {
                                var inp = !string.IsNullOrEmpty(request.Parameters) ?
                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters) : new Dictionary<string, string>();
                                inp.Add("card_id", input.pg_card_id.ToString());
                                request.Parameters = JsonConvert.SerializeObject(inp);
                            }

                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            clientBl.ApproveDepositFromPaymentSystem(request, false);
                            PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                            BaseHelpers.BroadcastBalance(request.ClientId.Value);
                        }

                        var response = new CheckOutput
                        {
                            Status = PayBoxHelpers.Statuses.Ok,
                            Salt = CommonFunctions.GetRandomString(16)
                        };
                        signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(response, ";"), secKey);
                        response.Signature = CommonFunctions.ComputeMd5(signature);
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                        return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var error = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error
                };
                if (ex.Detail == null)
                {
                    error.ErrorCode = PayBoxHelpers.GetErrorCode(Constants.Errors.GeneralException);
                    error.ErrorDescription = ex.Message;
                }
                else
                {
                    error.ErrorCode = PayBoxHelpers.GetErrorCode(ex.Detail.Id);
                    error.ErrorDescription = ex.Detail.Message;
                }
                WebApiApplication.DbLogger.Error(new Exception(JsonConvert.SerializeObject(error)));

                return CommonFunctions.ConvertObjectToXml(error, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error,
                    ErrorCode = PayBoxHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorDescription = ex.Message
                };
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        [HttpPost]
        [Route("api/Paybox/WithdrawResult")]
        public HttpResponseMessage WithdrawResult(XmlInput xmlInput)
        {
            WebApiApplication.DbLogger.Info("XMLInput = " + xmlInput.pg_xml);
            try
            {
                var deserializer = new XmlSerializer(typeof(WithdrawResultInput), new XmlRootAttribute("response"));
                using (var stringReader = new StringReader(xmlInput.pg_xml))
                {
                    var input = (WithdrawResultInput)deserializer.Deserialize(stringReader);
                    using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(paymentSystemBl))
                        {
                            using (var documentBl = new DocumentBll(clientBl))
                            {
                                using (var notificationBl = new NotificationBll(clientBl))
                                {
                                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBox);
                                    if (paymentSystem == null)
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.pg_order_id));
                                    if (request == null)
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                    var client = CacheManager.GetClientById(request.ClientId.Value);
                                    if (client == null)
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

                                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                    if (partnerPaymentSetting == null)
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);

                                    var sign = input.pg_sig;
                                    input.pg_sig = null;
                                    var script = "WithdrawResult";
                                    var secKey = partnerPaymentSetting.Password;
                                    string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(input, ";"), secKey);
                                    input.pg_sig = CommonFunctions.ComputeMd5(signature);
                                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input.pg_sig));
                                    if (input.pg_sig != sign)
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);

                                    if (request.Amount != Convert.ToDecimal(input.pg_payment_amount))
                                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);

                                    if (input.pg_status.ToLower() == "ok")//success
                                    {
                                        request.ExternalTransactionId = input.pg_payment_id;
                                        paymentSystemBl.ChangePaymentRequestDetails(request);
                                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty, request.CashDeskId,
                                            null, true, request.Parameters, documentBl, notificationBl);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                        BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                        /*var serializer = new JavaScriptSerializer();
                                        var cardInfo = serializer.Deserialize<CardInfo>(request.Info);
                                        DeleteCard(Convert.ToInt64(cardInfo.pg_card_id), client.Id);*/
                                    }

                                    var response = new CheckOutput
                                    {
                                        Status = PayBoxHelpers.Statuses.Ok,
                                        Salt = CommonFunctions.GetRandomString(16)
                                    };
                                    signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(response, ";"), secKey);
                                    response.Signature = CommonFunctions.ComputeMd5(signature);
                                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                                    return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var error = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error
                };
                if (ex.Detail == null)
                {
                    error.ErrorCode = Constants.Errors.GeneralException;
                    error.ErrorDescription = ex.Message;
                }
                else
                {
                    error.ErrorCode = ex.Detail.Id;
                    error.ErrorDescription = ex.Detail.Message;
                }
                WebApiApplication.DbLogger.Error(new Exception(JsonConvert.SerializeObject(error)));

                return CommonFunctions.ConvertObjectToXml(error, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error,
                    ErrorCode = PayBoxHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorDescription = ex.Message
                };
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        [HttpPost]
        [Route("api/Paybox/AddCardResult")]
        public HttpResponseMessage AddCardResult(XmlInput xmlInput)
        {
            WebApiApplication.DbLogger.Info("XMLInput = " + xmlInput.pg_xml);
            try
            {
                var deserializer = new XmlSerializer(typeof(AddCardResultInput), new XmlRootAttribute("response"));
                using (var stringReader = new StringReader(xmlInput.pg_xml))
                {
                    var input = (AddCardResultInput)deserializer.Deserialize(stringReader);
					using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
					{
						var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.pg_order_id));
						if (request == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(request.ClientId.Value);
						if (client == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

						var sign = input.pg_sig;
						input.pg_sig = null;
						var script = "AddCardResult";
						var secKey = GetPaymentSetting(client.PartnerId, request.CurrencyId, string.Empty).Password;
						string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(input, ";"), secKey);
						input.pg_sig = CommonFunctions.ComputeMd5(signature);
						WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input.pg_sig));
						if (input.pg_sig != sign)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);

						if (input.pg_status.ToLower() == "success")//success
						{
							request.Info = JsonConvert.SerializeObject(new
							{
								pg_card_id = input.pg_card_id.ToString(),
								pg_card_number = input.pg_card_hash
							});

							paymentSystemBl.ChangePaymentRequestDetails(request);
						}

						var response = new CheckOutput
						{
							Status = PayBoxHelpers.Statuses.Ok,
							Salt = CommonFunctions.GetRandomString(16)
						};
						signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(response, ";"), secKey);
						response.Signature = CommonFunctions.ComputeMd5(signature);
						WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
						return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
					}
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var error = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error
                };
                if (ex.Detail == null)
                {
                    error.ErrorCode = Constants.Errors.GeneralException;
                    error.ErrorDescription = ex.Message;
                }
                else
                {
                    error.ErrorCode = ex.Detail.Id;
                    error.ErrorDescription = ex.Detail.Message;
                }
                WebApiApplication.DbLogger.Error(new Exception(JsonConvert.SerializeObject(error)));

                return CommonFunctions.ConvertObjectToXml(error, Constants.HttpContentTypes.ApplicationXml);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new CheckOutput
                {
                    Status = PayBoxHelpers.Statuses.Error,
                    ErrorCode = PayBoxHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorDescription = ex.Message
                };
                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
            }
        }

        /*
        public static DeleteCardInfoOutput DeleteCard(long cardId, int clientId)
        {
            using (var partnerBl = new PartnerBll(WebApiApplication.Identity, WebApiApplication.DbLogger))
            {
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBox);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                var script = "remove";
                var merchantId = partnerPaymentSetting.UserName;
                var secKey = partnerPaymentSetting.Password;
                var url = partnerBl.GetPaymentValueByKey(null, paymentSystem.Id, Constants.PartnerKeys.PayBoxWithdrawUrl) +
                    string.Format("/v1/merchant/{0}/cardstorage/remove", merchantId);
                var salt = CommonFunctions.GetRandomString(16);
                var paymentRequestInput = new DeleteCardInfoInput
                {
                    pg_merchant_id = merchantId,
                    pg_user_id = clientId,
                    pg_card_id = cardId,
                    pg_salt = salt
                };
                string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);

                paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                var defaultProtocol = ServicePointManager.SecurityProtocol;
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var deserializer = new XmlSerializer(typeof(DeleteCardInfoOutput), new XmlRootAttribute("response"));
                using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, SecurityProtocolType.Tls12))
                {
                    var output = (DeleteCardInfoOutput)deserializer.Deserialize(stream);

                    ServicePointManager.SecurityProtocol = defaultProtocol;
                    return output;
                }
            }
        }
        
        public static DeleteCardInfoOutput GetCards(int clientId)
        {
            using (var partnerBl = new PartnerBll(WebApiApplication.Identity, WebApiApplication.DbLogger))
            {
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBox);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                var script = "list";
                var merchantId = partnerPaymentSetting.UserName;
                var secKey = partnerPaymentSetting.Password;
                var url = partnerBl.GetPaymentValueByKey(null, paymentSystem.Id, Constants.PartnerKeys.PayBoxWithdrawUrl) +
                    string.Format("/v1/merchant/{0}/cardstorage/list", merchantId);
                var salt = CommonFunctions.GetRandomString(16);
                var paymentRequestInput = new DeleteCardInfoInput
                {
                    pg_merchant_id = merchantId,
                    pg_user_id = clientId,
                    pg_salt = salt
                };
                string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);
                paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                var defaultProtocol = ServicePointManager.SecurityProtocol;
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var deserializer = new XmlSerializer(typeof(DeleteCardInfoOutput), new XmlRootAttribute("response"));
                using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, SecurityProtocolType.Tls12))
                {
                    var output = (DeleteCardInfoOutput)deserializer.Deserialize(stream);
                    ServicePointManager.SecurityProtocol = defaultProtocol;
                    return output;
                }
            }
        }
        */

        private BllPartnerPaymentSetting GetPaymentSetting(int partnerId, string currencyId, string payboxPaymnentSystem)
        {
            var paymentSystem = CacheManager.GetPaymentSystemByName(Integration.Payments.Helpers.PayBoxHelpers.GetPaymentSystem(payboxPaymnentSystem));
            if (paymentSystem == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystem.Id, currencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
            return partnerPaymentSetting;
        }
    }
}
