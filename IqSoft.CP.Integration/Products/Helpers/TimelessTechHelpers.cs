using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Platforms.Models.OASIS;
using IqSoft.CP.Integration.Products.Models.TimelessTech;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class TimelessTechHelpers
	{
		private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.TimelessTech);
		public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, log4net.ILog log)
		{
			var product = CacheManager.GetProductById(productId);
			var client = CacheManager.GetClientById(clientId);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var device = session.DeviceType == (int)DeviceTypes.Desktop ? "desktop" : "mobile";
			var mode = isForDemo ? "fun" : "real";
			var pragmaticLaunchUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechPragmaticLaunchUrl);
			var isPragmatic = product.SubProviderId == CacheManager.GetGameProviderByName(Constants.GameProviders.PragmaticPlay).Id;
			var launchUrl = isPragmatic ? pragmaticLaunchUrl : Provider.GameLaunchUrl;
			var gameId = product.ExternalId.Contains("lobby") ? $"lobby_id={product.ExternalId.Split('_')[1]}" : $"game_id={product.ExternalId}";
			var url = $"{launchUrl}/?mode={mode}&{gameId}&language={session.LanguageId}&operator_id={operatorID}&device={device}";
			if (isForDemo)
				return url;
			else
			   return $"{url}&currency={client.CurrencyId}&token={token}";
		}

		public static List<Game> GetGames(int partnerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"games{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = headers,
				Url = $"{url}/games/list/all"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var games = JsonConvert.DeserializeObject<Data>(res);
			var list = games.games.GroupBy(x => x.vendor).Select(y => y.Key);
			var lobbies = GetLobbies(partnerId);
			games.games.AddRange(lobbies);
			return games.games;
		}

		public static List<Game> GetLobbies(int partnerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"lobbies{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = headers,
				Url = $"{url}/lobbies/list/all"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var lobbies = JsonConvert.DeserializeObject<Lobbies>(res);
			var games = new List<Game>();
			foreach (var item in lobbies.lobbies)
			{
				var game = new Game
				{
					title = $"lobby_{item.lobby_id}",
					vendor = item.vendor,
					platform = item.platform,
					subtype = item.subtype,
					details = item.details,
				};
				games.Add(game);
			}
			return games;
		}

		public static string GetSignature(int partnerId, string timestamp, int clientId, string clientCurrencyId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var signature = clientId == 0 ? CommonFunctions.ComputeSha1($"relayer{timestamp}{sectetKey}")
												  : CommonFunctions.ComputeSha1($"relayer{clientId}{clientCurrencyId}{timestamp}{sectetKey}");
			return signature;
		}

		public static void CreateCampaign(FreeSpinModel freespinModel, ILog log)
		{
			var client = CacheManager.GetClientById(freespinModel.ClientId);
			var sectetKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/vendors"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var vendors = JsonConvert.DeserializeObject<Vendor>(res);
			var product = CacheManager.GetProductByExternalId(Provider.Id, freespinModel.ProductExternalId);
			if (!product.SubProviderId.HasValue)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
			var subProvider = CacheManager.GetGameProviderById(product.SubProviderId.Value).Name;
			var vendor = vendors.data.FirstOrDefault(x => subProvider.ToLower().Replace("livegaming", string.Empty).Replace("direc", string.Empty) == x.ToLower().Replace("-", string.Empty).Replace(" ", string.Empty) ||
															subProvider.ToLower().Replace("gaming", string.Empty).Replace("games", string.Empty) == x.ToLower().Replace("gaming", string.Empty) ||
															subProvider.Length > 5 && x.Length > 5 && subProvider.ToLower().Substring(0, 6) == x.ToLower().Replace("-", string.Empty).Replace(" ", string.Empty).Substring(0, 6));

			log.Info($"Vendor {vendor}");
			var data = new Campaign
			{
				vendor = vendor,
				campaign_code = freespinModel.BonusId.ToString(),
				freespins_per_player = freespinModel.SpinCount.Value,
				begins_at = ((DateTimeOffset)freespinModel.StartTime.AddMinutes(1)).ToUnixTimeSeconds(),
				expires_at = ((DateTimeOffset)freespinModel.FinishTime).ToUnixTimeSeconds(),
				currency_code = client.CurrencyId,
				players = client.Id.ToString(),
				games = new	List<GameModel>
				{ 
					new GameModel
					{
						game_id = Convert.ToInt32(freespinModel.ProductExternalId),
					    total_bet = 1
					}					
				}
			};
			log.Info($"Data {JsonConvert.SerializeObject(data)}");
			httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/create",
				PostData = JsonConvert.SerializeObject(data)
			};
			res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}

		public static void CancelCampaign(int partnerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var data = new
			{
				campaign_code = "",
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/cancel",
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}

		public static void AddPlayersToCampaign(int partnerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var data = new
			{
				campaign_code = "",
				players = "",
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/players/add",
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}

		public static void RemovePlayersFromCampaign(int partnerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var data = new
			{
				campaign_code = "",
				players = "",
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/players/remove",
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}
	}
}
