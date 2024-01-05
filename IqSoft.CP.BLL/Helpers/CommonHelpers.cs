using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.BLL.Helpers
{
    public static class CommonHelpers
    {
        public static int? GetCountryId(int id, string language)
        {
            int groupId = id;
            while (true)
            {
                var r = CacheManager.GetRegionById(groupId, language);
                if (r == null || r.Id == 0)
                    return null;
                if (r.TypeId == (int)RegionTypes.Country)
                    return r.Id;
                if(r.ParentId == null)
                    return null;
                groupId = r.ParentId.Value;
            }
        }
    }
}
