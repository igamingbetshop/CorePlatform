using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.BLL.Services
{
    public class RegionBll : PermissionBll, IRegionBll
    {
        #region Constructors

        public RegionBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public RegionBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public Region SaveRegion(DAL.Region region)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateRegion,
                ObjectTypeId = (int)ObjectTypes.Region,
                ObjectId = region.Id
            });
            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(region.Id))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var dbRegion = Db.Regions.FirstOrDefault(x => x.Id == region.Id);
            var isParentChanged = false;
            var isNewStateBlocked = false;

            if (dbRegion == null)
            {
                region.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Region,
                    Text = region.NickName,
                    LanguageId = Constants.DefaultLanguageId
                });
                region.State = (int)RegionStates.Active;
                region.Path = string.Empty;
                Db.Regions.Add(region);
                Db.SaveChanges();
                dbRegion = region;
            }
            else
            {
                isParentChanged = dbRegion.ParentId != region.ParentId;
                isNewStateBlocked = dbRegion.State != region.State && region.State == (int)RegionStates.Inactive;

                dbRegion.ParentId = region.ParentId;
                dbRegion.TypeId = region.TypeId;
                dbRegion.NickName = region.NickName;
                dbRegion.IsoCode = region.IsoCode;
                dbRegion.IsoCode3 = region.IsoCode3;
                dbRegion.State = region.State;
                dbRegion.CurrencyId = region.CurrencyId;
                dbRegion.LanguageId = region.LanguageId;
                dbRegion.Info = region.Info;
                Db.SaveChanges();
            }

            string parentPath = String.Empty;
            if (dbRegion.ParentId.HasValue)
            {
                var parentRegion = Db.Regions.FirstOrDefault(x => x.Id == dbRegion.ParentId.Value);
                if (parentRegion.State == (int)RegionStates.Inactive && region.State != (int)RegionStates.Inactive)
                    throw CreateException(LanguageId, Constants.Errors.ParentRegionInactive);
                parentPath = parentRegion.Path;
            }
            dbRegion.Path = string.Format("{0}{1}\\", parentPath, dbRegion.Id);
            if (isParentChanged)
                ChangeRegionPaths(dbRegion);
            if (isNewStateBlocked)
                BlockChildRegions(dbRegion);
            Db.SaveChanges();
            return dbRegion;
        }

        public Region GetRegionByCountryCode(string code)
        {
            return Db.Regions.FirstOrDefault(x => x.IsoCode == code && x.TypeId == (int)RegionTypes.Country);
        }
        public fnRegion GetRegionByName(string countryName, string languageId)
        {
            return Db.fn_Region(languageId).FirstOrDefault(x => (x.NickName.ToLower() == countryName.ToLower() || x.Name.ToLower() == countryName.ToLower()) &&
                                                                 x.TypeId == (int)RegionTypes.Country);
        }

        public fnRegion GetfnRegionById(int id, string languageId = null)
        {
            var langId = languageId ?? LanguageId;
            return Db.fn_Region(langId).FirstOrDefault(x => x.Id == id);
        }

        public List<fnRegion> GetfnRegions(FilterRegion filter, string languageId, bool checkPermission, int? partnerId)
        {
            if (checkPermission)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewRegion,
                    ObjectTypeId = (int)ObjectTypes.Region
                });
                if (!checkPermissionResult.HaveAccessForAllObjects)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var resp = filter.FilterObjects(Db.fn_Region(languageId)).ToList();
            if (partnerId.HasValue)
            {
                var partnerCountrySettings = CacheManager.GetPartnerCountrySettings(partnerId.Value, (int)PartnerCountrySettingTypes.BlockedForRegistration);
				resp = resp.Where(x => !partnerCountrySettings.Any(y => y.RegionId == x.Id)).ToList();
            }
            return resp;
        }

        private void ChangeRegionPaths(Region region)
        {
            var childRegions = Db.Regions.Where(x => x.ParentId == region.Id).ToList();
            foreach (var childRegion in childRegions)
            {
                childRegion.Path = string.Format("{0}{1}\\", region.Path, childRegion.Id);
                ChangeRegionPaths(childRegion);
            }
        }

        private void BlockChildRegions(Region region)
        {
            var childRegions = Db.Regions.Where(x => x.ParentId == region.Id).ToList();
            foreach (var childRegion in childRegions)
            {
                childRegion.State = (int)RegionStates.Inactive;
                ChangeRegionPaths(childRegion);
            }
        }

        public List<fnRegionPath> GetRegionPath(int regionId)
        {
            return Db.fn_RegionPath(regionId).ToList();
        }
        public Dictionary<int, string> GetAllCountryCodes()
        {
            return Db.Regions.Where(x => x.TypeId == (int)RegionTypes.Country && !string.IsNullOrEmpty(x.IsoCode)).ToDictionary(x => x.Id, x => x.IsoCode);
        }
    }
}
