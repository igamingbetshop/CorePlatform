using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using System;
using log4net;
using Microsoft.EntityFrameworkCore;

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
            try
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreatePartnerLanguageSetting,
                    ObjectTypeId = ObjectTypes.Language,
                    ObjectId = partnerLanguageSetting.Id
                });

                if (!checkPermissionResult.HaveAccessForAllObjects &&
                    !checkPermissionResult.AccessibleObjects.AsEnumerable().Contains(partnerLanguageSetting.Id))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                var currentTime = DateTime.UtcNow;
                var state = partnerLanguageSetting.State == (int)PartnerLanguageStates.Active ? (int)PartnerLanguageStates.Active : (int)PartnerLanguageStates.Inactive;
                var dbPartnerLanguageSetting = Db.PartnerLanguageSettings.FirstOrDefault(x => x.Id == partnerLanguageSetting.Id);

                if (dbPartnerLanguageSetting == null)
                {
                    dbPartnerLanguageSetting = new PartnerLanguageSetting
                    {
                        CreationTime = currentTime,
                        PartnerId = partnerLanguageSetting.PartnerId,
                        LanguageId = partnerLanguageSetting.LanguageId,
                        State = state,
                        LastUpdateTime = currentTime
                    };
                    Db.PartnerLanguageSettings.Add(dbPartnerLanguageSetting);
                }
                else
                {
                    dbPartnerLanguageSetting.LastUpdateTime = currentTime;
                    dbPartnerLanguageSetting.State = state;
                }
                Db.SaveChanges();
                return dbPartnerLanguageSetting;
            }
            catch(Exception e)
            {
                Log.Error(e);
                return new PartnerLanguageSetting();
            }
        }
    }
}
