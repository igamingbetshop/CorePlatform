using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using System.Collections.Generic;
using IqSoft.CP.PaymentGateway.Models.ExternalCashier;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System;
using Newtonsoft.Json;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.Linq;
using IqSoft.CP.DAL;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class ExternalCashierController : ControllerBase
    {
        private readonly static int PaymentSystemId = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.ExternalCashier).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            ""
        };

        [HttpPost]
        [Route("api/ExternalCashier/Authentication")]
        public ActionResult Authentication(AuthenticationInput input)
        {
            var result = new AuthenticationOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);

                if (!int.TryParse(input.ClientId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                if (partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Active)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
                var sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}", clientId, partnerPaymentSetting.Password));
                if (input.Signature.ToLower() != sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
                   client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
                using (var regionBl = new RegionBll(new DAL.Models.SessionIdentity(), Program.DbLogger))
                {
                    var regionPath = regionBl.GetRegionPath(client.RegionId);
                    var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                    var cityPath = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
                    var city = string.Empty;
                    if (cityPath != null)
                        city = CacheManager.GetRegionById(cityPath.Id ?? 0, client.LanguageId)?.Name;
                    result.CountryCode = country?.IsoCode;
                    result.City = city;
                    result.Id = clientId;
                    result.UserName = client.UserName;
                    result.FirstName = client.FirstName;
                    result.LastName = client.LastName;
                    result.Email = client.Email;
                    result.MobileNumber = client.MobileNumber;
                    result.BirthDate = client.BirthDate.ToString();
                    result.Address = client.Address;
                    result.ZipCode = client.ZipCode.Trim();
                    result.Language = client.LanguageId;
                    result.State = Enum.GetName(typeof(ClientStates), Convert.ToInt32(client.State));
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);

                result.Code = ex.Detail.Id;
                result.Description = ex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                result.Code = Constants.Errors.GeneralException;
                result.Description = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("api/ExternalCashier/Payment")]
        public ActionResult Payment(PaymentInput input)
        {
            var result = new PaymentOutput();
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                if (!int.TryParse(input.ClientId, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                var sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}{4}", input.Amount, clientId, client.CurrencyId,
                                                                                       input.TransactionId, partnerPaymentSetting.Password));
                if (input.Signature.ToLower() != sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
                   client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
                if (input.Amount <= 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var paymentRequest = new PaymentRequest
                {
                    Amount = input.Amount,
                    ClientId = clientId,
                    CurrencyId = client.CurrencyId,
                    PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                    PartnerPaymentSettingId = partnerPaymentSetting.Id,
                    ExternalTransactionId = input.TransactionId
                };

                using (var paymentSystemBl = new PaymentSystemBll(new DAL.Models.SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var regionBl = new RegionBll(paymentSystemBl))
                        {
                            var regionPath = regionBl.GetRegionPath(client.RegionId);
                            var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                            var cityPath = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
                            var city = string.Empty;
                            if (cityPath != null)
                                city = CacheManager.GetRegionById(cityPath.Id ?? 0, client.LanguageId)?.Name;
                            var paymentInfo = new PaymentInfo
                            {
                                Country = country?.IsoCode,
                                City = city,
                            };
                            paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            });
                            paymentRequest.CountryCode = country?.IsoCode;
                            using (var scope = CommonFunctions.CreateTransactionScope())
                            {
                                var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest);
                                PaymentHelpers.InvokeMessage("PaymentRequst", request.ClientId);
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                scope.Complete();
                                result.OrderId = request.Id.ToString();
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);

                result.Code = ex.Detail.Id;
                result.Description = ex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                result.Code = Constants.Errors.GeneralException;
                result.Description = ex.Message;
            }
            return Ok(result);
        }
    }
}