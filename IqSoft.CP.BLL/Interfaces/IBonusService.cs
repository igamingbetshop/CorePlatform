using IqSoft.CP.DAL;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IBonusService : IBaseBll
    {
        Bonu CreateBonus(Bonu bonus, decimal? percent);
        Bonu UpdateBonus(Bonu bon);
        List<fnBonus> GetBonuses(int? partnerId, int? type, int? status);		
	    void CalculateCashBackBonus();
	    List<int> AwardCashbackBonus(DateTime lastExecutionTime);
        List<int> GiveBonusToAffiliateManagers();
    }
}
