using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;

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
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetClientsInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard();
                response.ResponseObject = reportBl.GetMembersInfoForDashboard(filter);
                return response;
            }
        }

        private static ApiResponseBase GetPaymentsInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard();
                response.ResponseObject = reportBl.GetMemberPaymentsForDashboard(filter);
                return response;
            }
        }
    }
}
