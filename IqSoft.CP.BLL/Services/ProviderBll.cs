using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.BLL.Services
{
    public class ProviderBll : PermissionBll, IProviderBll
    {
        #region Constructors

        public ProviderBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public ProviderBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public List<AffiliatePlatform> GetAffiliatePlatforms()
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliatePlatforms
            });

            return Db.AffiliatePlatforms.ToList();
        }

        public List<NotificationService> GetNotificationServices()
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewNotificationServices
            });

            return Db.NotificationServices.ToList();
        }
    }
}
