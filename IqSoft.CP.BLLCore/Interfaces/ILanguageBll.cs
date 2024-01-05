using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using System.Collections.Generic;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface ILanguageBll : IBaseBll
    {
        List<DAL.Language> GetLanguages();

        List<PartnerLanguageSetting> GetPartnerLanguages(int partnerId);

        PartnerLanguageSetting SavePartnerLanguageSetting(PartnerLanguageSetting partnerLanguageSetting);
    }
}
