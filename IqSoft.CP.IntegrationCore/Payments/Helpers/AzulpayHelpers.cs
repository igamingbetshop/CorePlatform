using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Azulpay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class AzulpayHelpers
    {
        public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var regionBl = new RegionBll(clientBl))
                        {
                            var client = CacheManager.GetClientById(input.ClientId);
                            if (string.IsNullOrEmpty(client.MobileNumber))
                                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                            if (string.IsNullOrEmpty(client.Address))
                                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);

                            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Azulpay);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var requestHeaders = new Dictionary<string, string>
                            {
                               { "secret", partnerPaymentSetting.Password}
                            };
                            var url = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.AzulpayUrl);
                            var apiKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.AzulpayApiKey);
                            var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                            var regionPath = regionBl.GetRegionPath(client.RegionId);
                            var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country && x.IsoCode != null);
                            var isoCode = country != null ? country.IsoCode : regionPath.FirstOrDefault(x => x.IsoCode != null).IsoCode;
                            var postData = new PaymentInput
                            {
                                ApiKey = apiKey,
                                LastName = client.Id.ToString(),
                                FirstName = client.UserName,
                                Email = client.Email,
                                Phone = client.MobileNumber,
                                Address = client.Address,
                                City = client.Address,
                                State = client.Address,
                                Country = isoCode,
                                ZipCode = string.IsNullOrWhiteSpace(client.ZipCode) ? "12345" : client.ZipCode,
                                Amount = input.Amount.ToString(),
                                Currency = input.CurrencyId,
                                OrderId = input.Id.ToString(),
                                ClientIp = string.IsNullOrEmpty(session.LoginIp) ? Constants.DefaultIp : session.LoginIp,
                                RedirectUrl = cashierPageUrl,
                                WebhookUrl = string.Format("{0}/api/Azulpay/ApiRequest", paymentGateway),
                            };
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationJson,
                                RequestMethod = HttpMethod.Post,
                                Url = url,
                                RequestHeaders = requestHeaders,
                                PostData = JsonConvert.SerializeObject(postData)
                            };
                            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                            var output = JsonConvert.DeserializeObject<PaymentOutput>(response);

                            var data = new Data();
                            if (output.Message == "INITIATED")
                            {
                                data = JsonConvert.DeserializeObject<Data>(output.Data.ToString());
                                input.ExternalTransactionId = data.PaymentId;
                                paymentSystemBl.ChangePaymentRequestDetails(input);
                                return data.RedirectUrl;
                            }
                            if (output.Message == "DECLINED" || output.Message == "ERROR")
                            {
                                data = JsonConvert.DeserializeObject<Data>(output.Data.ToString());
                                throw new Exception($"Error: {output.Message} {data.GatewayResponse}");
                            }
                            else
                                throw new Exception($"Error: {output.Message}");                           
                        }
                    }
                }
            }
        }
    }
}
