using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AgentWebApi.Models;
using log4net;
using Newtonsoft.Json;
using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Helpers;
using System.Linq;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Caching;
using System;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class BetShopController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetBetShops":
                    return GetBetShops(JsonConvert.DeserializeObject<ApiFilterBetShop>(request.RequestData), identity, log);
                case "GetBetShopById":
                    return GetBetShopById(Convert.ToInt32(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetBetShops(ApiFilterBetShop filter, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var betShopBl = new BetShopBll(identity, log))
            {
                using (var userBl = new UserBll(identity, log))
                {
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                        userBl.CheckPermission(Constants.Permissions.ViewBetShop);

                    var input = filter.MaptToFilterfnBetShop();
                    input.AgentIds = new Common.Models.FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new System.Collections.Generic.List<Common.Models.FiltersOperationType>
                        {
                            new Common.Models.FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                IntValue = isAgentEmploye ? user.ParentId.Value : identity.Id
                            }
                        }
                    };
                    var betshops = betShopBl.GetBetShopsPagedModel(input, false);

                    var response = new ApiResponseBase
                    {
                        ResponseObject = new { Count = betshops.Count, Entities = betshops.Entities.Select(x => x.TofnBetshopModel(betShopBl.GetUserIdentity().TimeZone)).ToList() }
                    };
                    return response;
                }
            }
        }

        private static ApiResponseBase GetBetShopById(int id, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var betShopBl = new BetShopBll(identity, log))
            {
                using (var userBl = new UserBll(identity, log))
                {
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                        userBl.CheckPermission(Constants.Permissions.ViewBetShop);

                    var betshop = betShopBl.GetBetShopById(id, false);
                    if (betshop.UserId != (isAgentEmploye ? user.ParentId.Value : identity.Id))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);

                    var cashDesks = betShopBl.GetCashDesks(new FilterCashDesk { BetShopId = betshop.Id }, false);
                    betshop.CashDesks = cashDesks;

                    var betShopGroups = betShopBl.GetBetShopGroups(new FilterBetShopGroup { }, false);
                    betshop.BetShopGroup = betShopGroups.FirstOrDefault(x => x.Id == betshop.GroupId);

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

                        var bsh = betshop.ToBetshopModel(betShopBl.GetUserIdentity().TimeZone);

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
        }
    }
}