using System;
using System.Linq;
using log4net;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.CurrencyModels;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static  class CurrencyController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetCurrencies":
                    return GetCurrencies(identity, log);
                case "GetCurrencyRates":
                    return GetCurrencyRates(JsonConvert.DeserializeObject<string>(request.RequestData), identity, log);
                case "SaveCurrency":
                    return SaveCurrency(JsonConvert.DeserializeObject<CurrencyModel>(request.RequestData), identity, log);
                case "GetPartnerCurrencySettings":
                    return GetPartnerCurrencySettings(Convert.ToInt32(request.RequestData), identity, log);
                case "SavePartnerCurrencySetting":
                    return
                        SavePartnerCurrencySetting(
                            JsonConvert.DeserializeObject<ApiPartnerCurrencySetting>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase SaveCurrency(CurrencyModel currency, SessionIdentity identity, ILog log)
        {
            using var currencyBl = new CurrencyBll(identity, log);
            var result = currencyBl.SaveCurrency(currency.MapToCurrency()).MapToCurrency();
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.Currencies, result.Id));
            return new ApiResponseBase
            {
                ResponseObject = result
            };
        }

        private static ApiResponseBase GetCurrencyRates(string currencyId, SessionIdentity identity, ILog log)
        {
            using (var currencyBl = new CurrencyBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = currencyBl.GetCurrencyRatesByCurrencyId(currencyId).OrderByDescending(x => x.Id).Select(x => x.MapToCurrencyRateModel(identity.TimeZone)).ToList()
                };
            }
        }

        private static ApiResponseBase GetCurrencies(SessionIdentity identity, ILog log)
        {
            using (var currencyBl = new CurrencyBll(identity, log))
            {
                var currencies = currencyBl.GetCurrencies();
                return new ApiResponseBase
                {
                    ResponseObject =
                    (from x in currencies
                     let rate = (x.CurrentRate != 0m ? 1 / x.CurrentRate : 0)
                     select new
                     {
                         Id = x.Id,
                         CurrentRate = rate > 10000 ? Math.Round(rate, 0) : (rate>1 ? Math.Round(rate, 2) : Math.Round(rate, 8)),
                         Symbol = x.Symbol,
                         SessionId = x.SessionId,
                         CreationTime = x.CreationTime,
                         LastUpdateTime = x.LastUpdateTime,
                         Code = x.Code,
                         Name = x.Name
                     }).ToList()
                };
            }
        }

        private static ApiResponseBase GetPartnerCurrencySettings(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var currencyBl = new CurrencyBll(identity, log))
            {
                var partnerCurrencies = currencyBl.GetPartnerCurrencies(partnerId).Select(x => x.MapToPartnerCurrencySettingModel(identity.TimeZone)).OrderBy(x => x.Priority).ToList();
                var ids = partnerCurrencies.Select(x => x.CurrencyId).ToList();
                var currencies = currencyBl.GetCurrencies().Where(x => !ids.Contains(x.Id)).Select(x => x.Id).ToList();

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        partnerCurrencies,
                        currencies
                    }
                };
            }
        }

        private static ApiResponseBase SavePartnerCurrencySetting(ApiPartnerCurrencySetting partnerCurrencySetting, SessionIdentity identity, ILog log)
        {
            using (var currencyBl = new CurrencyBll(identity, log))
            {
                var result = currencyBl.SavePartnerCurrencySetting(partnerCurrencySetting.ToPartnerCurrencySetting());
                return new ApiResponseBase
                {
                    ResponseObject = result.ToApiPartnerCurrencySetting()
                };
            }
        }
    }
}