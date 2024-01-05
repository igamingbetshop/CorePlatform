using log4net;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using System.Net;
using IqSoft.CP.Common.Models;
using System.Web.Script.Serialization;
using IqSoft.CP.BLL.Caching;

namespace IqSoft.CP.BLL.Helpers
{
    public static class CaptchaHelpers
    {
		public static CaptchaOutput CallCaptchaApi(string input, SessionIdentity session)
		{
			var url = CacheManager.GetNotificationServiceValueByKey(0, Constants.PartnerKeys.CaptchaUrl, (int)NotificationServices.Captcha);
			var secretKey = CacheManager.GetConfigKey(session.PartnerId, Constants.PartnerKeys.CaptchaSecretKey);

			url = url + "?secret=" + secretKey + "&response=" + input;
			var response = (new WebClient()).DownloadString(url);
			var serializer = new JavaScriptSerializer();
			var output = serializer.Deserialize<CaptchaOutput>(response);
			return output;
		}
    }
}
