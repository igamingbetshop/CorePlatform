using IqSoft.CP.AdminWebApi.ClientModels.Models;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Filters.Affiliate;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.Filters;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System.Linq;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class AffiliateController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetAffiliates":
                    return GetfnAffiliates(
                        JsonConvert.DeserializeObject<ApiFilterfnAffiliate>(request.RequestData), identity, log);
                case "GetAffiliateById":
                    return GetAffiliateById(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "UpdateAffiliate":
                    return UpdateAffiliate(
                        JsonConvert.DeserializeObject<ApiFnAffiliateModel>(request.RequestData), identity, log);
                case "GetAffiliateAccounts":
                    return GetAffiliateAccounts(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "UpdateCommissionPlan":
                    return UpdateCommissionPlan(JsonConvert.DeserializeObject<Common.Models.AffiliateModels.ApiAffiliateCommission>(request.RequestData), identity, log);
                case "GetTransactions":
                    return GetTransactions(JsonConvert.DeserializeObject<ApiFilterfnAgentTransaction>(request.RequestData), identity, log);
                case "CreateDebitCorrection":
                    return CreateDebitCorrection(JsonConvert.DeserializeObject<TransferInput>(request.RequestData), identity, log);
                case "CreateCreditCorrection":
                    return CreateCreditCorrection(JsonConvert.DeserializeObject<TransferInput>(request.RequestData), identity, log);
                case "GetAffiliateCorrections":
                    return GetAffiliateCorrections(JsonConvert.DeserializeObject<ApiFilterAffiliateCorrection>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetfnAffiliates(ApiFilterfnAffiliate filter, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                var input = filter.MapToFilterfnAffiliate(identity.TimeZone);
                var resp = affiliateBl.GetfnAffiliates(input);
                return new ApiResponseBase
                {
                    ResponseObject = new { resp.Count, Entities = resp.Entities.Select(x => x.ToApifnAffiliateModel(identity.TimeZone)).ToList() }
                };
            }
        }

        private static ApiResponseBase GetAffiliateById(int id, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                var resp = affiliateBl.GetAffiliateById(id, true);
                return new ApiResponseBase
                {
                    ResponseObject = resp.ToApifnAffiliateModel(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase UpdateAffiliate(ApiFnAffiliateModel input, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
			{
				var partner = CacheManager.GetPartnerById(identity.PartnerId);
				identity.Domain = partner.SiteUrl.Split(',')[0];
				var resp = affiliateBl.UpdateAffiliate(input.ToFnAffiliate());
				return new ApiResponseBase();
            }
        }

        public static ApiResponseBase GetAffiliateAccounts(int affiliateId, SessionIdentity identity, ILog log)
        {
            using (var affiliateService = new AffiliateService(identity, log))
            {
                var accounts = affiliateService.GetAffiliateAccounts(affiliateId).Select(x => x.ToFnAccountModel()).ToList();
                return new ApiResponseBase
                {
                    ResponseObject = accounts
                };
            }
        }

        private static ApiResponseBase UpdateCommissionPlan(Common.Models.AffiliateModels.ApiAffiliateCommission input, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                affiliateBl.UpdateCommissionPlan(input);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase CreateDebitCorrection(TransferInput transferInput, SessionIdentity identity, ILog log)
        {
            using (var affiliateService = new AffiliateService(identity, log))
            {
                using (var documentBl = new DocumentBll(affiliateService))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = affiliateService.CreateDebitOnAffiliate(transferInput, documentBl).MapToDocumentModel(identity.TimeZone)
                    };
                }
            }
        }

        private static ApiResponseBase CreateCreditCorrection(TransferInput transferInput, SessionIdentity identity, ILog log)
        {
            using (var affiliateService = new AffiliateService(identity, log))
            {
                using (var documentBl = new DocumentBll(affiliateService))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = affiliateService.CreateCreditOnAffiliate(transferInput, documentBl).MapToDocumentModel(identity.TimeZone)
                    };
                }
            }
        }

        private static ApiResponseBase GetTransactions(ApiFilterfnAgentTransaction apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetAffiliateTransactions(apiFilter.ToFilterfnAffiliateTransaction(identity.TimeZone), apiFilter.AffiliateId);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.ToApifnAffiliateTransaction(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetAffiliateCorrections(ApiFilterAffiliateCorrection apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetAffiliateCorrections(apiFilter.MapToFilterAffiliateCorrection(identity.TimeZone));
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => new
                        {
                            x.Id,
                            x.PartnerId,
                            x.AffiliateId,
                            x.FirstName,
                            x.LastName,
                            x.Amount,
                            x.CurrencyId,
                            x.Creator,
                            x.CreatorFirstName,
                            x.CreatorLastName,
                            x.OperationTypeName,
                            CreationTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone),
                            LastUpdateTime = x.LastUpdateTime.GetGMTDateFromUTC(identity.TimeZone)
                        }).ToList()
                        
                       
                    }
                };
            }
        }
    }
}