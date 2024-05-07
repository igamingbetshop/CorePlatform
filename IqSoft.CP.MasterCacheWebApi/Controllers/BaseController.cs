using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.MasterCacheWebApi.ControllerClasses;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using System.Web.Http;

namespace IqSoft.CP.MasterCacheWebApi.Controllers
{
    public class BaseController : ApiController
    {
        protected BllProduct CheckProductAvailability(int partnerId, int productId)
        {
            var product = CacheManager.GetProductById(productId);
            if (product == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
            if (product.State == (int)ProductStates.Inactive || product.State == (int)ProductStates.DisabledByProvider)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductBlockedForThisPartner);

            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
            if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
            if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductBlockedForThisPartner);
            if (product.GameProviderId == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

            return product;
        }

        protected static object CreateUrl(GetProductUrlInput input, BllProduct product, BllGameProvider provider, SessionIdentity clientSession, string token)
        {
            var partner = CacheManager.GetPartnerById(input.PartnerId);
            var providerName = provider.Name.ToLower();
            if (providerName == Constants.GameProviders.Internal.ToLower())
                return InternalHelpers.GetUrl(product, partner, token, provider, input, clientSession);
            else if (providerName == Constants.GameProviders.IqSoft.ToLower())
                return IqSoftHelpers.GetUrl(provider, partner, product, (input.IsForMobile.HasValue && input.IsForMobile.Value),
                    token, input, clientSession, WebApiApplication.DbLogger);
            else if (providerName == Constants.GameProviders.TwoWinPower.ToLower())
                return TwoWinPowerHelpers.GetUrl(input.PartnerId, input.ProductId, token, input.ClientId, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.DriveMedia.ToLower())
                return DriveMediaHelpers.GetUrl(input, clientSession.CurrencyId, token);
            else if (providerName == Constants.GameProviders.BetGames.ToLower())
                return BetGamesHelpers.GetUrl(token, input.PartnerId, input.IsForDemo, input.IsForMobile ?? false, clientSession);
            else if (providerName == Constants.GameProviders.Ezugi.ToLower())
                return EzugiHelpers.GetUrl(partner.Id, token, input.IsForDemo, product, clientSession);
            else if (providerName == Constants.GameProviders.TomHorn.ToLower())
            {
                var launchUrl = Integration.Products.Helpers.TomHornHelpers.GetGameData(input.PartnerId, input.ClientId, input.ProductId,
                    (input.IsForMobile.HasValue && input.IsForMobile.Value), input.IsForDemo, clientSession, out string newSessionId);
                if (!input.IsForDemo)
                    ProductController.GetProductSession(input.ProductId, (input.IsForMobile.HasValue && input.IsForMobile.Value) ? (int)DeviceTypes.Mobile : (int)DeviceTypes.Desktop, clientSession, newSessionId);
                return launchUrl;
            }
            else if (providerName == Constants.GameProviders.ZeusPlay.ToLower())
                return Integration.Products.Helpers.ZeusPlayHelpers.GetUrl(partner.Id, input.ProductId, input.ClientId, token, input.IsForDemo);
            else if (providerName == Constants.GameProviders.Singular.ToLower())
                return SingularHelpers.GetUrl(partner.Name, token, (input.IsForMobile.HasValue && input.IsForMobile.Value), clientSession);
            else if (providerName == Constants.GameProviders.EvenBet.ToLower())
                return EvenBetHelpers.GetUrl(partner.Id, input.ClientId, input.LanguageId, input.IsForDemo);
            else if (providerName == Constants.GameProviders.EkkoSpin.ToLower())
                return EkkoSpinHelpers.GetUrl(partner, input.ClientId, input.ProductId, provider.GameLaunchUrl, input.LanguageId);
            else if (providerName == Constants.GameProviders.SkyWind.ToLower())
                return SkyWindHelpers.GetUrl(input.PartnerId, input.ProductId, clientSession.Id, token, input.LanguageId);
            else if (providerName == Constants.GameProviders.ISoftBet.ToLower())
                return ISoftBetHelpers.GetUrl(input.ProductId, token, input.ClientId, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.SolidGaming.ToLower())
                return SolidGamingHelpers.GetUrl(input.PartnerId, input.ProductId, token, input.ClientId, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.YSB.ToLower())
                return YSBHelpers.GetUrl(input.PartnerId, input.ProductId, token, input.ClientId, input.LanguageId, (input.IsForMobile.HasValue && input.IsForMobile.Value), input.IsForDemo);
            else if (providerName == Constants.GameProviders.InBet.ToLower())
                return InBetHelpers.GetUrl(partner.Id, input.ProductId, token, input.ClientId, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.CModule.ToLower())
                return CModuleHelpers.GetUrl(partner, input.ProductId, token, input.ClientId, (input.IsForMobile.HasValue && input.IsForMobile.Value), input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.SunCity.ToLower())
                return SunCityHelpers.GetUrl(input.PartnerId, input.ProductId, input.ClientId, (input.IsForMobile.HasValue && input.IsForMobile.Value), clientSession);
            else if (providerName == Constants.GameProviders.ESport.ToLower())
                return ESportHelpers.GetUrl(input.PartnerId, input.ProductId, token, input.ClientId);
            else if (providerName == Constants.GameProviders.Igrosoft.ToLower())
            {
                string externalToken;
                var url = IgrosoftHelpers.GetUrl(input.PartnerId, input.ProductId, input.ClientId, token, input.LanguageId, input.IsForDemo, clientSession, out externalToken);
                if (!input.IsForDemo)
                    ProductController.GetProductSession(input.ProductId, (input.IsForMobile.HasValue && input.IsForMobile.Value) ? (int)DeviceTypes.Mobile : (int)DeviceTypes.Desktop, clientSession, externalToken);
                return url;
            }
            else if (providerName == Constants.GameProviders.Endorphina.ToLower())
                return Integration.Products.Helpers.EndorphinaHelpers.GetLaunchUrl(input.PartnerId, token, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger);
            else if (providerName == Constants.GameProviders.Ganapati.ToLower())
                return GanapatiHelpers.GetUrl(input.PartnerId, input.ProductId, token, input.ClientId, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.Evolution.ToLower())
                return EvolutionHelpers.GetUrl(input.ProductId, token, input.ClientId, (input.IsForMobile.HasValue && input.IsForMobile.Value), input.Ip, clientSession);
            else if (providerName == Constants.GameProviders.TVBet.ToLower())
                return TVBetHelpers.GetUrl(token, partner.Id, clientSession);
            else if (providerName == Constants.GameProviders.OutcomeBet.ToLower() || providerName == Constants.GameProviders.Mascot.ToLower())
                return OutcomeBetHelpers.GetUrl(input.PartnerId, input.ClientId, input.ProductId, input.LanguageId, input.IsForDemo, token, clientSession, WebApiApplication.DbLogger);
            else if (providerName == Constants.GameProviders.PragmaticPlay.ToLower())
                return Integration.Products.Helpers.PragmaticPlayHelpers.GetSessionUrl(partner.Id, product, token, (input.IsForMobile.HasValue && input.IsForMobile.Value), input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.LuckyGames.ToLower())
                return LuckyGamesHelpers.GetUrl(input.ProductId, token, input.LanguageId, input.IsForDemo);
            else if (providerName == Constants.GameProviders.SoftGaming.ToLower())
                return Integration.Products.Helpers.SoftGamingHelpers.GetUrl(input.PartnerId, input.ProductId, token, input.ClientId, input.IsForDemo,
                    (input.IsForMobile.HasValue && input.IsForMobile.Value), clientSession, WebApiApplication.DbLogger);

            else if (providerName == Constants.GameProviders.BlueOcean.ToLower())
            {
                var resp = Integration.Products.Helpers.BlueOceanHelpers.GetUrl(input.PartnerId, input.ProductId, input.ClientId, 
                    input.IsForDemo, clientSession, WebApiApplication.DbLogger, out string externalToken);
                if (!input.IsForDemo)
                    ProductController.GetProductSession(input.ProductId, (input.IsForMobile.HasValue && input.IsForMobile.Value) ? (int)DeviceTypes.Mobile : (int)DeviceTypes.Desktop, clientSession, externalToken);
                return resp;
            }
            else if (providerName == Constants.GameProviders.SmartSoft.ToLower())
                return SmartSoftHelpers.GetUrl(token, partner.Id, input.ProductId, input.ClientId, input.IsForDemo, input.IsForMobile ?? false, clientSession);
            else if (providerName == Constants.GameProviders.SoftSwiss.ToLower())
                return Integration.Products.Helpers.SoftSwissHelpers.GetUrl(input.PartnerId, input.ProductId, input.ClientId, 
                    input.IsForDemo, input.IsForMobile ?? false, clientSession, WebApiApplication.DbLogger);
            else if (providerName == Constants.GameProviders.Kiron.ToLower())
                return Integration.Products.Helpers.KironHelpers.GetUrl(token, input.PartnerId, clientSession.CurrencyId, input.IsForDemo, input.IsForMobile ?? false, false, clientSession);
            else if (providerName == Constants.GameProviders.BetSoft.ToLower())
                return Integration.Products.Helpers.BetSoftHelpers.GetSessionUrl(input.PartnerId, product, token, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.AWC.ToLower())
                return Integration.Products.Helpers.AWCHelpers.GetUrl(input.PartnerId, product.Id, input.ClientId, token, input.IsForDemo, input.IsForMobile ?? false, clientSession);
            else if (providerName == Constants.GameProviders.Habanero.ToLower())
                return HabaneroHelpers.GetUrl(token, input.PartnerId, product.Id, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.Evoplay.ToLower())
                return EvoplayHelpers.GetUrl(input.PartnerId, input.ClientId, token, product.Id, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.GMW.ToLower())
                return GMWHelpers.GetUrl(token, input.PartnerId, product.Id, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.BetSolutions.ToLower())
                return BetSolutionsHelpers.GetUrl(token, input.PartnerId, product.Id, input.IsForDemo, input.IsForMobile ?? false, clientSession);
            else if (providerName == Constants.GameProviders.GrooveGaming.ToLower())
                return GrooveHelpers.GetUrl(input.ClientId, token, input.PartnerId, product.Id, input.IsForDemo, input.IsForMobile ?? false, clientSession);
            else if (providerName == Constants.GameProviders.Betsy.ToLower())
                return BetsyHelpers.GetUrl(token, input.PartnerId, clientSession);
            else if (providerName == Constants.GameProviders.PropsBuilder.ToLower())
                return PropsBuilderHelpers.GetUrl(input.ClientId, token, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.Racebook.ToLower())
                return RacebookHelpers.GetUrl(input.ClientId, token, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.EveryMatrix.ToLower())
                return EveryMatrixHelpers.GetUrl(input.ClientId, token, input.PartnerId, product.Id, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.Nucleus.ToLower())
                return NucleusHelpers.GetUrl(token, input.PartnerId, product.Id, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.Mancala.ToLower())
                return Integration.Products.Helpers.MancalaHelpers.GetGameLaunchUrl(input.PartnerId, input.ClientId, token, product.Id, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.VisionaryiGaming.ToLower())
                return VisionaryiGamingHelpers.GetUrl(token, input.PartnerId, clientSession);
            else if (providerName == Constants.GameProviders.DragonGaming.ToLower())
                return Integration.Products.Helpers.DragonGamingHelpers.GetGameLaunchUrl(input.ClientId, token, input.PartnerId, input.ProductId, input.IsForMobile ?? false, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.GoldenRace.ToLower())
                return GoldenRaceHelpers.GetUrl(token, input.PartnerId, input.ProductId, input.IsForDemo, input.IsForMobile ?? false, clientSession);
            else if (providerName == Constants.GameProviders.Mahjong.ToLower())
                return MahjongHelpers.GetUrl(token, input.PartnerId, input.ProductId, clientSession);
            else if (providerName == Constants.GameProviders.LuckyGaming.ToLower())
                return Integration.Products.Helpers.LuckyGamingHelpers.GetUrl(input.PartnerId, input.ClientId, input.ProductId);
            else if (providerName == Constants.GameProviders.BAS.ToLower() || providerName == Constants.GameProviders.DGS.ToLower())
                return BASHelpers.GetUrl(input.PartnerId,input.ProductId, token, input.ClientId, clientSession);
            else if (providerName == Constants.GameProviders.IPTGaming.ToLower())
                return Integration.Products.Helpers.IPTGamingHelpers.GetUrl(input.ClientId, token, input.ProductId, clientSession);
            else if (providerName == Constants.GameProviders.TurboGames.ToLower())
                return TurboGamesHelpers.GetUrl(token, input.PartnerId, input.ProductId, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.JackpotGaming.ToLower())
                return Integration.Products.Helpers.JackpotGamingHelpers.GetUrl(token, input.ClientId, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger);
            else if (providerName == Constants.GameProviders.AleaPlay.ToLower())
                return Integration.Products.Helpers.AleaPlayHelpers.GetUrl(token, input.ClientId, input.PartnerId, input.ProductId, input.IsForDemo, clientSession);
            else if (providerName == Constants.GameProviders.PlaynGo.ToLower())
                return PlaynGoHelpers.GetUrl(token, input.PartnerId, input.ProductId, input.IsForDemo, input.IsForMobile ?? false, clientSession);
            else if (providerName == Constants.GameProviders.Elite.ToLower())
                return Integration.Products.Helpers.EliteHelpers.GetUrl(token, input.ClientId, input.PartnerId, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger);
            else if (providerName == Constants.GameProviders.SoftLand.ToLower())
                return Integration.Products.Helpers.SoftLandHelpers.GetUrl(token, input.ClientId, input.PartnerId, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger);            
            else if (providerName == Constants.GameProviders.BGGames.ToLower())
                return Integration.Products.Helpers.BGGamesHelpers.GetUrl(token, input.ClientId, input.PartnerId, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger);            
            else if (providerName == Constants.GameProviders.TimelessTech.ToLower() || providerName == Constants.GameProviders.BCWGames.ToLower())
                return Integration.Products.Helpers.TimelessTechHelpers.GetUrl(token, input.ClientId, input.PartnerId, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger);      
            else if (providerName == Constants.GameProviders.RiseUp.ToLower())
                return Integration.Products.Helpers.RiseUpHelpers.GetUrl(token, input.ClientId, input.PartnerId, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger); 
            else if (providerName == Constants.GameProviders.LuckyStreak.ToLower())
                return Integration.Products.Helpers.LuckyStreakHelpers.GetUrl(token, input.ClientId, input.PartnerId, input.ProductId, input.IsForDemo, clientSession, WebApiApplication.DbLogger);
            else
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
        }
    }
}