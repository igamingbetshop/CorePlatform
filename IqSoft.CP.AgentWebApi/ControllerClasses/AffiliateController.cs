using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models;
using log4net;
using System.Linq;
using System;
using Newtonsoft.Json;
using IqSoft.CP.AgentWebApi.Models.User;
using IqSoft.CP.DAL.Models.User;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class AffiliateController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetReferralLinks":
                    return GetReferralLinks(identity, log);
                case "GenerateNewLink":
                    return GenerateNewLink(identity, log);
                case "GetAffiliateById":
                    return GetAffiliateById(identity, log);
                case "GetBalance":
                    return GetBalance(identity, log);
                case "ChangePassword":
                    return ChangePassword(JsonConvert.DeserializeObject<ChangePasswordInput>(request.RequestData), identity, log);
                case "CreatePayoutRequest":
                    return CreatePayoutRequest(JsonConvert.DeserializeObject<ClientCorrectionInput>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetReferralLinks(SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                var resp = affiliateBl.GetReferralLinks();
                var partner = CacheManager.GetPartnerById(identity.PartnerId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        MaximumAvailable = Constants.AffiliateMaximumLinksCount,
                        Entities = resp.Select(x => x.ToApiReferralLink(partner, identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GenerateNewLink(SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                var resp = affiliateBl.GenerateNewLink();
                var partner = CacheManager.GetPartnerById(identity.PartnerId);

                return new ApiResponseBase
                {
                    ResponseObject = resp.ToApiReferralLink(partner, identity.TimeZone)
                };
            }
        }
      
        private static ApiResponseBase GetAffiliateById(SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                var affiliate = affiliateBl.GetAffiliateById(identity.Id, false);

                return new ApiResponseBase
                {
                    ResponseCode = Constants.SuccessResponseCode,
                    ResponseObject = affiliate.ToApifnAffiliateModel(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetBalance(SessionIdentity identity, ILog log)
        {
            if (identity.IsAffiliate)
            {
                var partner = CacheManager.GetPartnerById(identity.PartnerId);
                var affiliateBalances = BaseBll.GetObjectBalance((int)ObjectTypes.Affiliate, identity.Id);
                var balance = Math.Floor(affiliateBalances.Balances.Sum(x => BaseBll.ConvertCurrency(x.CurrencyId, partner.CurrencyId, x.Balance)) * 100) / 100;
                return new ApiResponseBase
                {
                    ResponseObject = balance
                };
            }
            else
            {
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.AffiliateNotFound
                };
            }
        }

        private static ApiResponseBase ChangePassword(ChangePasswordInput changePasswordInput, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                affiliateBl.ChangeAffiliatePassword(identity.Id, changePasswordInput.OldPassword, changePasswordInput.NewPassword);
                CacheManager.RemoveUserFromCache(identity.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.User, identity.Id));
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase CreatePayoutRequest(ClientCorrectionInput clientCorrectionInput, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                using (var documentBl = new DocumentBll(affiliateBl))
                {
                    var affiliate = affiliateBl.GetAffiliateById(identity.Id, false);
                    if (affiliate.ClientId == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                    clientCorrectionInput.ClientId = affiliate.ClientId.Value;
                    return new ApiResponseBase
                    {
                        ResponseObject = affiliateBl.CreateDebitToAffiliateClient(clientCorrectionInput, documentBl).MapToDocumentModel(identity.TimeZone)
                    };
                }
            }
        }
    }
}