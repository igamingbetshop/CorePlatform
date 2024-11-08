﻿using System.Linq;
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
                case Constants.RequestMethods.GetPlayersInfo:
                    return GetPlayersInfo(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetBetsInfo:
                    return GetBetsInfo(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.GetProviderBets:
                    return GetProviderBets(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetDeposits:
                    return GetDeposits(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.GetWithdrawals:
                    return GetWithdrawals(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetOnlineClients:
                    return GetOnlineClients(JsonConvert.DeserializeObject<ApiFilterRealTime>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetClientsInfoList:
                    return
                        GetClientsInfoList(
                            JsonConvert.DeserializeObject<ApiFilterfnClientDashboard>(request.RequestData), identity, log);
                case Constants.RequestMethods.GetTopRegistrationCountries:
                    return GetTopRegistrationCountries(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopVisitorCountries:
                    return GetTopVisitorCountries(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopTurnoverClients:
                    return GetTopTurnoverClients(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopProfitableClients:
                    return GetTopProfitableClients(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopDamagingClients:
                    return GetTopDamagingClients(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopActiveClients:
                    return GetTopActiveClients(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopDepositMethods:
                    return GetTopDepositMethods(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopWithdrawMethods:
                    return GetTopWithdrawMethods(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopBonusReceivers:
                    return GetTopBonusReceivers(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopGrantedBonuses:
                    return GetTopGrantedBonuses(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopProfitableAgents:
                    return GetTopProfitableAgents(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopDamagingAgents:
                    return GetTopDamagingAgents(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopProfitablePartners:
                    return GetTopProfitablePartners(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.GetTopDamagingPartners:
                    return GetTopDamagingPartners(JsonConvert.DeserializeObject<ApiFilterDashboard>(request.RequestData),
                        identity, log);
                case Constants.RequestMethods.ExportClientsInfoList:
                    return
                        ExportClientsInfoList(
                            JsonConvert.DeserializeObject<ApiFilterfnClientDashboard>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetPlayersInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetPlayersInfoForDashboard(filter).MapToApiPlayersInfo();
                return response;
            }
        }

        private static ApiResponseBase GetBetsInfo(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetBetsInfoForDashboard(filter).MapToApiBetsInfo();
                return response;
            }
        }

        private static ApiResponseBase GetDeposits(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                var responseList =  reportBl.GetPaymentRequestsForDashboard(filter, (int)PaymentRequestTypes.Deposit);
                response.ResponseObject = responseList.Select(x => x.ToApiDepositsInfo()).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetWithdrawals(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                var responseList = reportBl.GetPaymentRequestsForDashboard(filter, (int)PaymentRequestTypes.Withdraw);
                response.ResponseObject = responseList.Select(x => x.ToApiWithdrawalsInfo()).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetProviderBets(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetProviderBetsForDashboard(filter).MapToApiProvidersBetsInfo();
                return response;
            }
        }

        private static ApiResponseBase GetOnlineClients(ApiFilterRealTime input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterRealTime(identity.TimeZone);

                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetOnlineClients(filter).MapToApiRealTimeInfo(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetClientsInfoList(ApiFilterfnClientDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetClientsInfoList(input.MapToFilterfnClientDashboard(identity.TimeZone))
                };
            }
        }

        private static ApiResponseBase GetTopRegistrationCountries(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopRegistrationCountries(filter).OrderByDescending(x => x.TotalCount).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopVisitorCountries(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopVisitorCountries(filter).OrderByDescending(x => x.TotalCount).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopTurnoverClients(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopTurnoverClients(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopProfitableClients(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopProfitableClients(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopDamagingClients(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopDamagingClients(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopActiveClients(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopActiveClients(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopDepositMethods(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopDepositMethods(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopWithdrawMethods(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopWithdrawMethods(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopBonusReceivers(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopBonusReceivers(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopGrantedBonuses(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopGrantedBonuses(filter).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopProfitableAgents(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopAgents(filter, true).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopDamagingAgents(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopAgents(filter, false).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopProfitablePartners(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopPartners(filter, true).ToList();
                return response;
            }
        }

        private static ApiResponseBase GetTopDamagingPartners(ApiFilterDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase();
                var filter = input.MapToFilterDashboard(identity.TimeZone);
                response.ResponseObject = reportBl.GetTopPartners(filter, false).ToList();
                return response;
            }
        }

        private static ApiResponseBase ExportClientsInfoList(ApiFilterfnClientDashboard input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterfnClientDashboard(identity.TimeZone);
                var clients = reportBl.ExportClientsInfoList(filter);
                string fileName = "ExportPlayersDashboard.csv";
                string fileAbsPath = reportBl.ExportToCSV<DashboardClientInfo>(fileName, clients, input.FromDate, input.ToDate, reportBl.GetUserIdentity().TimeZone, input.AdminMenuId);

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
