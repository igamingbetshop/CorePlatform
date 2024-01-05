using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System.Data.Entity;
using IqSoft.CP.BLL.Helpers;

namespace IqSoft.CP.BLL.Services
{
    public class CurrencyBll : PermissionBll, ICurrencyBll
    {
        #region Constructors

        public CurrencyBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public CurrencyBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public List<Currency> GetCurrencies(bool checkPermission)
        {
            if (checkPermission)
            {
                var currencyAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewCurrency
                });
                if (!currencyAccess.HaveAccessForAllObjects)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var resp = Db.Currencies.ToList();

            return resp;
        }

        public Currency SaveCurrency(Currency currency)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateCurrency,
                ObjectTypeId = ObjectTypes.Currency,
                ObjectId = 1
            });
            var currentTime = GetServerDate();
            var dbCurrency = Db.Currencies.FirstOrDefault(x => x.Id == currency.Id);

            if (dbCurrency == null)
            {
                dbCurrency = new Currency
                {
                    Id = currency.Id,
                    CurrentRate = currency.CurrentRate,
                    Symbol = currency.Symbol,
                    CreationTime = currentTime,
                    LastUpdateTime = currentTime,
                    Code = currency.Code,
                    Name = currency.Id,
                    SessionId = SessionId
                };
                Db.Currencies.Add(dbCurrency);
            }
            else
            {
                var currencyRate = new CurrencyRate
                {
                    CurrencyId = currency.Id,
                    RateAfter = currency.CurrentRate,
                    SessionId = SessionId,
                    LastUpdateTime = currentTime,
                    CreationTime = currentTime
                };

                currency.Code = dbCurrency.Code;
               // currency.Name = dbCurrency.Name;
                currencyRate.RateBefore = dbCurrency.CurrentRate;
                currency.CreationTime = dbCurrency.CreationTime;
                currency.LastUpdateTime = currentTime;
                currency.SessionId = dbCurrency.SessionId;
                Db.Entry(dbCurrency).CurrentValues.SetValues(currency);
                Db.CurrencyRates.Add(currencyRate);
            }
            Db.SaveChanges();
            CacheManager.RemoveCurrencyById(currency.Id);
            return dbCurrency;
        }

        public List<CurrencyRate> GetCurrencyRatesByCurrencyId(string currencyId)
        {
            return Db.CurrencyRates.Include(x => x.UserSession.User).Where(x => x.CurrencyId == currencyId).ToList();
        }

        public List<PartnerCurrencySetting> GetPartnerCurrencies(int partnerId)
        {
			return CacheManager.GetPartnerCurrencies(partnerId).Select(x => new PartnerCurrencySetting {
				Id = x.Id,
				PartnerId = x.PartnerId,
				CurrencyId = x.CurrencyId,
				State = x.State,
				CreationTime = x.CreationTime,
				LastUpdateTime = x.LastUpdateTime,
                Priority = x.Priority ?? 0,
                UserMinLimit = x.UserMinLimit,
                UserMaxLimit = x.UserMaxLimit,
                ClientMinBet = x.ClientMinBet
            }).ToList();
        }

        public PartnerCurrencySetting SavePartnerCurrencySetting(PartnerCurrencySetting partnerCurrencySetting)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerCurrencySetting,
                ObjectTypeId = ObjectTypes.Currency,
                ObjectId = partnerCurrencySetting.Id
            });

            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.Contains(partnerCurrencySetting.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var currentTime = GetServerDate();

            var dbPartnerCurrencySetting = Db.PartnerCurrencySettings.FirstOrDefault(x => x.Id == partnerCurrencySetting.Id);

            if (dbPartnerCurrencySetting == null)
            {
                partnerCurrencySetting.CreationTime = currentTime;
                Db.PartnerCurrencySettings.Add(partnerCurrencySetting);
                Db.SaveChanges();
                dbPartnerCurrencySetting = partnerCurrencySetting;
            }
            else
            {
                dbPartnerCurrencySetting.State = partnerCurrencySetting.State;
                dbPartnerCurrencySetting.LastUpdateTime = currentTime;
                dbPartnerCurrencySetting.Priority = partnerCurrencySetting.Priority;
                dbPartnerCurrencySetting.UserMinLimit = partnerCurrencySetting.UserMinLimit;
                dbPartnerCurrencySetting.UserMaxLimit = partnerCurrencySetting.UserMaxLimit;
                dbPartnerCurrencySetting.ClientMinBet = partnerCurrencySetting.ClientMinBet;
                SaveChangesWithHistory((int)ObjectTypes.PartnerCurrencySetting, dbPartnerCurrencySetting.Id, JsonConvert.SerializeObject(dbPartnerCurrencySetting.ToPartnerCurrencyInfo()), string.Empty);
            }

            return dbPartnerCurrencySetting;
        }

        public Currency GetCurrencyById(string currencyId)
        {
            return Db.Currencies.Where(x => x.Id == currencyId).FirstOrDefault();
        }
    }
}
