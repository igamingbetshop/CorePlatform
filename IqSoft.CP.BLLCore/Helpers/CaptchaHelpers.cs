using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using System.Net;
using IqSoft.CP.Common.Models;
using IqSoft.CP.BLL.Caching;
using Newtonsoft.Json;
using System.Net.Http;

namespace IqSoft.CP.BLL.Helpers
{
    public static class CaptchaHelpers
    {
		public static CaptchaOutput CallCaptchaApi(string input, SessionIdentity session)
		{
			var url = CacheManager.GetNotificationServiceValueByKey(0, Constants.PartnerKeys.CaptchaUrl, (int)NotificationServices.Captcha);
			var secretKey = CacheManager.GetConfigKey(session.PartnerId, Constants.PartnerKeys.CaptchaSecretKey);

			url = url + "?secret=" + secretKey + "&response=" + input;
			using var httpClient = new HttpClient();
			var httpResponseMessage = httpClient.GetAsync(url).Result;
			var response = httpResponseMessage.Content.ReadAsStringAsync().Result;
			return JsonConvert.DeserializeObject<CaptchaOutput>(response);
		}
    }
}
