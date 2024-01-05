using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using System.Collections.Generic;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IRegionBll : IBaseBll
    {
        Region SaveRegion(Region region);
        fnRegion GetfnRegionById(int id, string languageId = null);
        List<fnRegion> GetfnRegions(FilterRegion filter, string languageId, bool checkPermission, int? partnerId);
    }
}