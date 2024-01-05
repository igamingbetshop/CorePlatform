using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Interfaces;
using IqSoft.CP.DAL.Models;
using log4net;
using System.Data.Entity;
using System;

namespace IqSoft.CP.BLL.Services
{
    public class LanguageBll : PermissionBll, ILanguageBll
    {
        #region Constructors

        public LanguageBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public LanguageBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public List<Language> GetLanguages()
        {
            var languageAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewLanguage
            });
            if (!languageAccess.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            return Db.Languages.ToList();
        }

        public List<PartnerLanguageSetting> GetPartnerLanguages(int partnerId)
        {
            return Db.PartnerLanguageSettings.Include(x => x.Language).Where(x => x.PartnerId == partnerId && x.State == (int)PartnerLanguageStates.Active).ToList();
        }

        public PartnerLanguageSetting SavePartnerLanguageSetting(PartnerLanguageSetting partnerLanguageSetting)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerLanguageSetting,
                ObjectTypeId = ObjectTypes.Language,
                ObjectId = partnerLanguageSetting.Id //??
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!checkPermissionResult.HaveAccessForAllObjects &&
                !checkPermissionResult.AccessibleObjects.Contains(partnerLanguageSetting.Id) ||
                (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerLanguageSetting.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (!Enum.IsDefined(typeof(PartnerLanguageStates), partnerLanguageSetting.State) ||
                !Db.Languages.Any(x => x.Id == partnerLanguageSetting.LanguageId))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var dbPartnerLanguageSetting = Db.PartnerLanguageSettings.FirstOrDefault(x => x.PartnerId == partnerLanguageSetting.PartnerId &&
                                                                                          x.LanguageId == partnerLanguageSetting.LanguageId);
            partnerLanguageSetting.LastUpdateTime = DateTime.UtcNow;
            if (dbPartnerLanguageSetting == null)
            {
                partnerLanguageSetting.CreationTime = partnerLanguageSetting.LastUpdateTime;
                Db.PartnerLanguageSettings.Add(partnerLanguageSetting);
                Db.SaveChanges();
                return partnerLanguageSetting;
            }

            dbPartnerLanguageSetting.State = partnerLanguageSetting.State;
            dbPartnerLanguageSetting.Order = partnerLanguageSetting.Order;
            Db.SaveChanges();
            return dbPartnerLanguageSetting;
        }
    }
}