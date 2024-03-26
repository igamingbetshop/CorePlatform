using IqSoft.CP.DAL;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using log4net;
using IqSoft.CP.BLL.Caching;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class UtilController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "SaveNote":
                    return SaveNote(JsonConvert.DeserializeObject<Note>(request.RequestData), identity, log);
                case "GetNotes":
                    return GetNotes(JsonConvert.DeserializeObject<ApiFilterNote>(request.RequestData), identity, log);
                case "GetClientProductsLimits":
                    return GetProductsLimits((int)ObjectTypes.Client,
                        JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "GetPartnerProductsLimits":
                    return GetProductsLimits((int)ObjectTypes.Partner,
                        JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "GetUserProductsLimits":
                    return GetProductsLimits((int)ObjectTypes.User,
                        JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "SaveProductLimit":
                    return SaveProductLimit(
                        JsonConvert.DeserializeObject<DAL.Models.ProductLimit>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase SaveNote(Note note, SessionIdentity identity, ILog log)
        {
            if (note.ObjectTypeId == (int)ObjectTypes.Client)
            {
                using (var clientbl = new ClientBll(identity, log))
                {
                    using (var userBl = new UserBll(identity, log))
                    {
                        clientbl.SaveNote(note);
                        return new ApiResponseBase
                        {
                            ResponseObject = note
                        };
                    }
                }
            }
            else if (note.ObjectTypeId == (int)ObjectTypes.Document)
            {
                using (var documentbl = new DocumentBll(identity, log))
                {
                    documentbl.SaveNote(note);

                    return new ApiResponseBase
                    {
                        ResponseObject = note
                    };
                }
            }
            using (var utilbl = new UtilBll(identity, log))
            {
                utilbl.SaveNote(note);

                return new ApiResponseBase
                {
                    ResponseObject = note
                };
            }
        }

        private static ApiResponseBase GetNotes(ApiFilterNote filter, SessionIdentity identity, ILog log)
        {
            using (var utilbl = new UtilBll(identity, log))
            {
                var notes = utilbl.GetNotes(filter.MapToFilterNote());

                return new ApiResponseBase
                {
                    ResponseObject = notes.MapToNoteModels(utilbl.GetUserIdentity().TimeZone)
                };
            }
        }

        private static ApiResponseBase GetProductsLimits(int objectTypeId, int objectId, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var limitType = (int)LimitTypes.FixedProductLimit;
                if (objectTypeId == (int)ObjectTypes.Client)
                    limitType = (int)LimitTypes.FixedClientMaxLimit;
                return new ApiResponseBase
                {
                    ResponseObject = productBl.GetProductsLimits(objectTypeId, objectId, limitType)
                };
            }
        }

        private static ApiResponseBase SaveProductLimit(DAL.Models.ProductLimit limit, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var result = productBl.SaveProductLimit(limit, true);
                var limitType = limit.ObjectTypeId == (int)ObjectTypes.Client ? (int)LimitTypes.FixedClientMaxLimit : (int)LimitTypes.FixedProductLimit;
                Helpers.Helpers.InvokeMessage("UpdateProductLimit", limit.ObjectTypeId, limit.ObjectId, limitType, limit.ProductId);
                return new ApiResponseBase
                {
                    ResponseObject = result
                };
            }
        }
    }
}