using IqSoft.CP.DAL;
using System.Collections.Generic;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface ICurrencyBll : IBaseBll
    {
        List<Currency> GetCurrencies(bool checkPermission);

        Currency SaveCurrency(Currency currency);

        List<CurrencyRate> GetCurrencyRatesByCurrencyId(string currencyId);

        List<PartnerCurrencySetting> GetPartnerCurrencies(int partnerId);

        PartnerCurrencySetting SavePartnerCurrencySetting(PartnerCurrencySetting partnerCurrencySetting);
    }
}