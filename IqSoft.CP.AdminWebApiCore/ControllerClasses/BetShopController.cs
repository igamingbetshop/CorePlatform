using System;
using System.Linq;
using Newtonsoft.Json;
using log4net;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.BetShopModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Filters.Reporting;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.DAL;
using IqSoft.CP.Common.Enums;
using Microsoft.AspNetCore.Hosting;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class BetShopController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            switch (request.Method)
            {
                case "GetBetShops":
                    return GetBetShops(JsonConvert.DeserializeObject<ApiFilterBetShop>(request.RequestData), identity, log);
                case "GetBetShopById":
                    return GetBetShopById(Convert.ToInt32(request.RequestData), identity, log);
                case "GetCashDesks":
                    return GetCashDesks(JsonConvert.DeserializeObject<ApiFilterCashDesk>(request.RequestData), identity, log);
                case "SaveBetShop":
                    return SaveBetShop(JsonConvert.DeserializeObject<ApiGetBetShopByIdOutput>(request.RequestData),
                         identity, log);
                case "SaveCashDesk":
                    return SaveCashDesk(JsonConvert.DeserializeObject<CashDeskModel>(request.RequestData), identity, log);
                case "GetCashDeskById":
                    return GetCashDeskById(Convert.ToInt32(request.RequestData), identity, log);
                case "GetBetShopGroups":
                    return GetBetShopGroups(JsonConvert.DeserializeObject<ApiFilterBetShopGroup>(request.RequestData),
                         identity, log);
                case "SaveBetshopGroup":
                    return SaveBetshopGroup(JsonConvert.DeserializeObject<BetshopGroupModel>(request.RequestData),
                         identity, log);
                case "DeleteBetShopGroup":
                    return DeleteBetShopGroup(JsonConvert.DeserializeObject<BetshopGroupModel>(request.RequestData),
                         identity, log);
                case "ChangeBetShopLimit":
                    return ChangeBetShopLimit(JsonConvert.DeserializeObject<ApiBetShopLimit>(request.RequestData),
                         identity, log);
                case "GetfnAdminShiftReportPaging":
                    return
                        GetfnAdminShiftReportPaging(
                            JsonConvert.DeserializeObject<ApiFilterShiftReport>(request.RequestData), identity, log);
                case "CashierIncasation":
                    return CashierIncasation(
                        JsonConvert.DeserializeObject<ApiCashierIncasationModel>(request.RequestData), identity, log);
                case "ExportShifts":
                    return ExportShiftReports(JsonConvert.DeserializeObject<ApiFilterShiftReport>(request.RequestData), identity, log, env);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetBetShops(ApiFilterBetShop filter, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var betshops = betShopBl.GetBetShopsPagedModel(filter.MaptToFilterfnBetShop());

                var response = new ApiResponseBase
                {
                    ResponseObject = new { Count = betshops.Count, Entities = betshops.Entities.MapBetshopfnExtendedModels(betShopBl.GetUserIdentity().TimeZone) }
                };
                return response;
            }
        }

        private static ApiResponseBase GetBetShopById(int id, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var betshop = betShopBl.GetBetShopById(id, false);
                var cashDesks = betShopBl.GetCashDesks(new FilterCashDesk { BetShopId = betshop.Id });
                betshop.CashDesks = cashDesks;

                var betShopGroups = betShopBl.GetBetShopGroups(new FilterBetShopGroup { });
                betshop.Group = betShopGroups.FirstOrDefault(x => x.Id == betshop.GroupId);

                using (var partnerBl = new PartnerBll(betShopBl))
                {
                    betshop.Partner = partnerBl.GetPartnerById(betshop.PartnerId);

                    var cashdeskstates = BaseBll.GetEnumerations("CashDeskStates", identity.LanguageId).Select(x => new EnumerationModel<int>
                    {
                        Id = x.Value,
                        Name = x.Text
                    }).ToList();
                    var states = BaseBll.GetEnumerations("BetShopStates", identity.LanguageId).Select(x => new EnumerationModel<int>
                    {
                        Id = x.Value,
                        Name = x.Text
                    }).ToList();
                    var groups = betShopGroups.Where(x => x.IsLeaf).Select(x => new EnumerationModel<int>
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).ToList();

                    var bsh = betshop.MapToBetshopExtendedModel(betShopBl.GetUserIdentity().TimeZone);

                    foreach (var cashDeskModel in bsh.CashDeskModels)
                    {
                        cashDeskModel.StateName = cashdeskstates.First(x => x.Id == cashDeskModel.State).Name;
                    }
                    bsh.StateName = states.First(x => x.Id == bsh.State).Name;

                    var response = new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            groups,
                            states,
                            cashdeskstates,
                            betshop = bsh
                        }
                    };
                    return response;
                }
            }
        }

        private static ApiResponseBase GetCashDesks(ApiFilterCashDesk filter, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var cashDesks = betShopBl.GetCashDesksPagedModel(filter.MapToFilterfnCashDesk());

                var response = new ApiResponseBase 
                { 
                    ResponseObject = new { Count = cashDesks.Count, Entities = cashDesks.Entities.Select(x => x.MapTofnCashDeskModel(betShopBl.Identity.TimeZone)).ToList() }
                };
                return response;
            }
        }

        private static ApiResponseBase SaveBetShop(ApiGetBetShopByIdOutput betShop, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                if (betShop.PartnerId == Constants.MainPartnerId)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);
                var response = new ApiResponseBase();
                var savedBetShop = betShopBl.SaveBetShop(betShop.MapToBetshop());

                var dataToSend = savedBetShop.MapToBetshopExtendedModel(betShopBl.GetUserIdentity().TimeZone);
                using (var partnerBl = new PartnerBll(betShopBl))
                {
                    dataToSend.PartnerName = partnerBl.GetPartnerById(savedBetShop.PartnerId).Name;

                    dataToSend.StateName = BaseBll.GetEnumerations(Constants.EnumerationTypes.BetShopStates, identity.LanguageId).First(x => x.Value == savedBetShop.State).Text;
                    dataToSend.GroupName = betShopBl.GetBetShopGroupById(savedBetShop.GroupId).Name;

                    response.ResponseObject = dataToSend;
                    return response;
                }
            }
        }

        private static ApiResponseBase SaveCashDesk(CashDeskModel cashDesk, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var response = new ApiResponseBase();
                var savedCashDesk = betShopBl.SaveCashDesk(cashDesk.MapToCashDesk(betShopBl.Identity.TimeZone)).MapToCashDeskModel(betShopBl.Identity.TimeZone);
                savedCashDesk.StateName = BaseBll.GetEnumerations("CashDeskStates", identity.LanguageId).First(x => x.Value == cashDesk.State).Text;
                response.ResponseObject = savedCashDesk;
                return response;
            }
        }

        private static ApiResponseBase GetCashDeskById(int id, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var response = new ApiResponseBase { ResponseObject = CacheManager.GetCashDeskById(id).MapToCashDeskModel(betShopBl.GetUserIdentity().TimeZone) };
                return response;
            }
        }

        private static ApiResponseBase GetBetShopGroups(ApiFilterBetShopGroup filter, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var betshopFilter = filter.MaptToFilterBetShopGroup();

                var response = new ApiResponseBase
                {
                    ResponseObject =
                        betShopBl.GetBetShopGroups(betshopFilter).MapToBetshopGroupModels(betShopBl.GetUserIdentity().TimeZone)
                };
                return response;
            }
        }

        private static ApiResponseBase SaveBetshopGroup(BetshopGroupModel model, SessionIdentity identity, ILog log)
        {
            using (var betshopBl = new BetShopBll(identity, log))
            {
                var timeZone = betshopBl.GetUserIdentity().TimeZone;
                if (model.PartnerId == Constants.MainPartnerId)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);

                model.IsLeaf = true;
                model.State = Constants.BetShopGroupStates.Active;

                var response = new ApiResponseBase
                {
                    ResponseObject = betshopBl.SaveBetShopGroup(model.MapToBetShopGroup(timeZone)).MapToBetshopGroupModel(timeZone)
                };
                return response;
            }
        }

        private static ApiResponseBase DeleteBetShopGroup(BetshopGroupModel model, SessionIdentity identity, ILog log)
        {
            using (var betshopBl = new BetShopBll(identity, log))
            {

                if (model.PartnerId == Constants.MainPartnerId)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);

                betshopBl.DeleteBetShopGroup(model.Id);

                var response = new ApiResponseBase();

                return response;
            }
        }

        private static ApiResponseBase ChangeBetShopLimit(ApiBetShopLimit betShopLimit, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                betShopBl.ChangeBetShopLimit(betShopLimit.MapToBetshopLimit(), identity.Id);

                var response = new ApiResponseBase();
                return response;
            }
        }

        private static ApiResponseBase GetfnAdminShiftReportPaging(ApiFilterShiftReport input, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var filter = input.MapToFilterfnAdminShiftReport();
				if (input.PartnerId != null && input.PartnerId.Value > 0)
					filter.PartnerIds = new FiltersOperation {
						IsAnd = true, OperationTypeList = new System.Collections.Generic.List<FiltersOperationType>
							{ new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = input.PartnerId.Value } }
					};

				var shiftReports = betShopBl.GetfnAdminShiftReportPaging(filter);

                var response = new ApiResponseBase
                {
                    ResponseObject = shiftReports.MapToApiShiftReportModel(identity.TimeZone)
                };
                return response;
            }
        }

        private static ApiResponseBase CashierIncasation(ApiCashierIncasationModel casherInfo, SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                betShopBl.CashierIncasation(casherInfo.CashDeskId, casherInfo.Amount);
            }
            var response = new ApiResponseBase
            {
                ResponseObject = new
                {

                }
            };
            return response;
        }

        private static ApiResponseBase ExportShiftReports(ApiFilterShiftReport input, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var betShopBl = new BetShopBll(clientBl))
                {
                    var timeZone = clientBl.GetUserIdentity().TimeZone;
                    var filter = input.MapToFilterfnAdminShiftReport();
                    var result = betShopBl.GetfnAdminShiftReports(filter);
                    string fileName = "ExporShiftReports.csv";
                    string fileAbsPath = clientBl.ExportToCSV<fnAdminShiftReport>(fileName, result, null, null, timeZone, env);

                    var response = new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            ExportedFilePath = fileAbsPath
                        }
                    };
                    return response;
                }
            } }
    }
}