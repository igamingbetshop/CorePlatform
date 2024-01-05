using System.Linq;
using log4net;
using Newtonsoft.Json;
using IqSoft.CP.Common;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.DAL.Models.PlayersDashboard;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class DashboardController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case Constants.RequestMethods.GetBetsInfo:
                    return GetBetsInfo(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.GetDeposits:
                    return GetDeposits(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.GetWithdrawals:
                    return GetWithdrawals(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetPlayersInfo:
                    return GetPlayersInfo(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetProviderBets:
                    return GetProviderBets(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetOnlineClients:
                    return GetOnlineClients(JsonConvert.DeserializeObject<ApiFilterRealTime>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetClientsInfoList:
                    return
                        GetClientsInfoList(
                            JsonConvert.DeserializeObject<ApiFilterfnClientDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.ExportClientsInfoList:
                    return
                        ExportClientsInfoList(
                            JsonConvert.DeserializeObject<ApiFilterfnClientDashboard>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetBetsInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard();
                response.ResponseObject = reportBl.GetBetsInfoForDashboard(filter).MapToApiBetsInfo();
                return response;
            }
        }

        private static ApiResponseBase GetDeposits(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard();
                var responseList =  reportBl.GetPaymentRequestsForDashboard(filter, (int)PaymentRequestTypes.Deposit);
                response.ResponseObject = responseList.Select(x => x.MapToApiPaymentRequestsInfo()).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetWithdrawals(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard();
                var responseList = reportBl.GetPaymentRequestsForDashboard(filter, (int)PaymentRequestTypes.Withdraw);
                response.ResponseObject = responseList.Select(x => x.MapToApiWithdrawalsInfo()).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetPlayersInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard();
                response.ResponseObject = reportBl.GetPlayersInfoForDashboard(filter).MapToApiPlayersInfo();
                return response;
            }
        }

        private static ApiResponseBase GetProviderBets(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard();
                response.ResponseObject = reportBl.GetProviderBetsForDashboard(filter).MapToApiProvidersBetsInfo();
                return response;
            }
        }

        private static ApiResponseBase GetOnlineClients(ApiFilterRealTime input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterRealTime();

                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetOnlineClients(filter).MapToApiRealTimeInfo()
                };
            }
        }

        private static ApiResponseBase GetClientsInfoList(ApiFilterfnClientDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetClientsInfoList(input.MapToFilterfnClientDashboard())
                };
            }
        }

        private static ApiResponseBase ExportClientsInfoList(ApiFilterfnClientDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterfnClientDashboard();
                var clients = reportBl.ExportClientsInfoList(filter);
                string fileName = "ExportPlayersDashboard.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiClientInfo>(fileName, clients, input.FromDate, input.ToDate, reportBl.GetUserIdentity().TimeZone, input.AdminMenuId);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }
    }
}
