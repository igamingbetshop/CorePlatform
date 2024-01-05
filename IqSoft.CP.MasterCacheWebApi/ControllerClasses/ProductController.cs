using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System;
using log4net;
using System.Linq;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;

namespace IqSoft.CP.MasterCacheWebApi.ControllerClasses
{
    public static class ProductController
    {
        public static GetProductSessionOutput GetProductSession(int productId, int? deviceType, SessionIdentity clientSession,
                                                                string token = null, int? maxLenght = null)
        {
            var client = CacheManager.GetClientById(clientSession.Id);
            var product = CacheManager.GetProductById(productId);
            if (product == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ProductNotFound);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, productId);
            if (partnerProductSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerProductSettingNotFound);
            if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ProductNotAllowedForThisPartner);
            var state = ClientBll.GetClientStateByProduct(client, product);
            if (state == (int)ClientStates.FullBlocked)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);

            var productSession = ClientBll.CreateNewProductSession(clientSession, out List<BllClientSession> oldSessions, productId, deviceType, token, maxLenght);
            foreach (var s in oldSessions)
            {
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_0", Constants.CacheItems.ClientSessions, s.Token));
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSessions, s.Token, s.ProductId));
            }
            var response = new GetProductSessionOutput
            {
                ProductToken = productSession.Token,
                ProductId = productId,
                Rating = partnerProductSetting.Rating
            };
            return response;
        }

        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity session, ILog log)
        {
            switch (request.Method)
            {
                case "AddToFavoriteList":
                    return
                        ChangeClientProductState(Convert.ToInt32(request.RequestData), request.ClientId, true, session, log);
                case "RemoveClientFavoriteProduct":
                    return
                        ChangeClientProductState(Convert.ToInt32(request.RequestData), request.ClientId, false, session, log);
                case "GetClientFavoriteProducts":
                    return
                        GetClientFavoriteProducts(request.ClientId, session, log);
                case "GetProductGroups":
                    return GetProductGroups(session);
                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }

        private static ApiResponseBase ChangeClientProductState(int productId, int clientId, bool isFavorite, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ChangeClientFavoriteProduct(clientId, productId, isFavorite);
                return new ApiResponseBase { ResponseObject = productId };
            }
        }

        private static ApiResponseBase GetClientFavoriteProducts(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var response = clientBl.GetClientFavoriteProducts(clientId);
                return new ApiResponseBase
                {
                    ResponseObject = response.Select(x => new ApiClientFavoritProduct
                    {
                        ClientId = x.ClientId,
                        ProductId = x.ProductId,
                        ProductName = x.Product.NickName,
                        GameProviderId = x.Product.GameProviderId.Value
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase GetProductGroups(SessionIdentity session)
        {
            return new ApiResponseBase
            {
                ResponseObject = CacheManager.GetProductCategories(session.PartnerId, session.LanguageId, (int)ProductCategoryTypes.ForClient)
                                             .Where(x => x.Type == (int)ProductCategoryTypes.ForClient)
                                             .Select(x => new { x.Id, x.Name }).OrderBy(x => x.Name).ToList()
            };
        }
    }
}