using IqSoft.CP.DAL;
using System.Collections.Generic;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IProviderBll : IBaseBll
    {
        List<AffiliatePlatform> GetAffiliatePlatforms();
        List<NotificationService> GetNotificationServices();
    }
}
