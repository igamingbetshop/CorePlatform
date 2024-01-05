using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Interkassa;
using log4net;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class InterkassaHelpers
    {
        public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId);
                        var partner = CacheManager.GetPartnerById(client.PartnerId);
                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Interkassa);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaUrl).StringValue;
                        var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                        var ckeckoutId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InterkassaCkeckoutId).StringValue;
                        var paymentMethod = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.InterkassaPaymentMethod);
                        var data = new
                        {
                            ik_co_id = ckeckoutId,
                            ik_cur = input.CurrencyId,
                            ik_am = input.Amount,
                            ik_pm_no = input.Id.ToString(),
                            ik_desc = partner.Name,
                            ik_act = "process",
                            ik_int = "json",
                            ik_payment_method = paymentMethod.Split('.')[0],
                            ik_payment_currency = paymentMethod.Split('.')[1],
                            ik_mode = "invoice",
                            ik_ia_u = string.Format("{0}/api/Interkassa/ApiRequest", paymentGateway),
                            ik_ia_m = "Post",
                            ik_suc_u = cashierPageUrl
                        };
                        var orderdParams = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}:{12}:",
                                                                 data.ik_act, data.ik_am, data.ik_co_id, data.ik_cur,
                                                                 data.ik_desc, data.ik_ia_m, data.ik_ia_u, data.ik_int,
                                                                 data.ik_mode, data.ik_payment_currency, data.ik_payment_method,
                                                                 data.ik_pm_no, data.ik_suc_u);
                        using (SHA256 sha256Hash = SHA256.Create())
                        {
                            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(orderdParams + "wU5vsPd1xR3QVSPdAgORnGjkCUgg2jVs"));
                            var signature = Uri.EscapeDataString(Convert.ToBase64String(bytes));
                            var ff = Convert.ToBase64String(bytes);
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = HttpMethod.Post,
                                Url = url,
                                PostData = CommonFunctions.GetUriEndocingFromObject(data) + $"&ik_sign={signature}"
                            };
                            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                            var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
                            if (output.ResultMsg == "Success")
                            {
                                return output.ResultData.PaymentForm.Action;
                            }
                            else
                            {
                                throw new Exception($"Error: {output.ResultCode} {output.ResultMsg}");
                            }
                        }
                    }
                }
            }
        }
    }
}

