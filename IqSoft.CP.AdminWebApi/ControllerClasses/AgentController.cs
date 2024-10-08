using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public class AgentController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetTransactions":
                    return GetTransactions(JsonConvert.DeserializeObject<ApiFilterfnAgentTransaction>(request.RequestData), identity, log);
                case "GetReportByAgents":
                    return GetReportByAgents(JsonConvert.DeserializeObject<ApiFilterfnAgent>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetTransactions(ApiFilterfnAgentTransaction apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var agentUser = CacheManager.GetUserById(identity.Id);
                
                var filter = apiFilter.ToFilterfnAgentTransaction(identity.TimeZone);
                var result = reportBl.GetAgentTransactions(filter, identity.Id, true);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.ToApifnAgentTransaction(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByAgents(ApiFilterfnAgent apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var agentUser = CacheManager.GetUserById(identity.Id);

                var filter = apiFilter.ToFilterfnUser(identity.TimeZone);
                var result = reportBl.GetAgentsReport(filter, true);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.ToApiAgentReportItem(identity.TimeZone)).ToList()
                    }
                };
            }
        }
    }
}
