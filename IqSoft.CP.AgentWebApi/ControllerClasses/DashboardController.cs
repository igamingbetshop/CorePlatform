

using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System.Linq;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class DashboardController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case Constants.RequestMethods.GetClientsInfo:
                    return GetClientsInfo(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.GetPaymentsInfo:
                    return GetPaymentsInfo(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.GetBetsInfo:
                    return GetBetsInfo(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetClientsInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();                    
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                if (!identity.IsAffiliate)
                    response.ResponseObject = reportBl.GetAgentMembersInfoForDashboard(filter);
                else
                {
                    filter.PartnerId = identity.PartnerId;
                    response.ResponseObject = reportBl.GetAffiliateMembersInfoForDashboard(filter);
                }
                return response;
            }
        }

        private static ApiResponseBase GetPaymentsInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                if (!identity.IsAffiliate)
                    response.ResponseObject = reportBl.GetAgentMemberPaymentsForDashboard(filter);
                else
                {
                    filter.PartnerId = identity.PartnerId;
                    response.ResponseObject = reportBl.GetAffiliateMemberPaymentsForDashboard(filter);
                }
                return response;
            }
        }

        private static ApiResponseBase GetBetsInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                if (!identity.IsAffiliate)
                    response.ResponseObject = reportBl.GetAgentMemberBetsInfoForDashboard(filter);
                else
                {
                    filter.PartnerId = identity.PartnerId;
                    //To Be Added
                }
                return response;
            }
        }
    }
}
