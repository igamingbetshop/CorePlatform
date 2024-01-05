using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Documents;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using Newtonsoft.Json;
using log4net;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class DocumentController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetTransactions":
                    return GetTransactions(JsonConvert.DeserializeObject<FilterTransaction>(request.RequestData),
                        identity, log);
                case "GetCurrencies":
                    return GetCurrencies(identity, log);//repeated
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetTransactions(FilterTransaction filter, SessionIdentity identity, ILog log)
        {
            using (var documentBl = new DocumentBll(identity, log))
            {
                var response = new ApiResponseBase { ResponseObject = documentBl.GetTransactions(filter) };
                return response;
            }
        }

        private static ApiResponseBase GetCurrencies(SessionIdentity identity, ILog log)
        {
            using (var currencyBl = new CurrencyBll(identity, log))
            {
                var response = new ApiResponseBase { ResponseObject = currencyBl.GetCurrencies(true) };
                return response;
            }
        }
    }
}