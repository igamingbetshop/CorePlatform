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
    }
}