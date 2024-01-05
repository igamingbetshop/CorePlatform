using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.MasterCacheWebApi.ControllerClasses;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
	public class GSoftHelpers
	{
		public static string GetUrl(GetProductUrlInput input, string gameLaunchUrl, SessionIdentity clientSession)
		{
			string productUrl = string.Format(gameLaunchUrl.Split(',')[0], input.LanguageId);
			if (!input.IsForDemo)
			{
				var client = CacheManager.GetClientById(input.ClientId);
				if (client == null)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

				var gSoftHelpers = new Integration.Products.Helpers.GSoftHelpers(client);
				gSoftHelpers.CreateMember();
				string providerToken = gSoftHelpers.LogIn();
				ProductController.GetProductSession(input.ProductId, (input.IsForMobile.HasValue && input.IsForMobile.Value) ? (int)DeviceTypes.Mobile : (int)DeviceTypes.Desktop, clientSession, providerToken);
				var domenPrefix = (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "ismart" : "mkt";
                var color = "og001";
                var adds = "";
                if( input.Position == "In-Play")
                 adds = (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "&type=1,0,l" : "&act=Esports&menutype=0&market=L";
                productUrl = string.Format(gameLaunchUrl.Split(',')[1], domenPrefix, input.LanguageId, providerToken, color, adds);
                WebApiApplication.DbLogger.Info("Gsoft GetUrl productUrl : " + productUrl);
            }
			return productUrl;
		}
	}
}