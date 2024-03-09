using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway;
using IqSoft.CP.ProductGateway.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.ProductGateway.Models.Fiverscool;

[EnableCors(origins: "*", headers: "*", methods: "POST")]
public class FiverscoolController : ApiController
{
	private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Fiverscool).Id;

	[HttpPost]
	[Route("{partnerId}/api/Fiverscool/ApiRequest")]
	public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
	{
		var response = string .Empty;
		var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
		WebApiApplication.DbLogger.Info(inputString);
		var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
		var input = JsonConvert.DeserializeObject<BaseInput>(inputString);
		try
		{
			var client = CacheManager.GetClientById(Convert.ToInt32(input.user_code)) ??
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
			var agentCode = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.FiverscoolAgentCode);
			var agentToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.FiverscoolAgentToken);
			var agentSecret = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.FiverscoolAgentSecret);
			if (agentSecret != input.agent_secret)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
			var product = new BllProduct();

			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out IqSoft.CP.DAL.Models.Cache.PartnerKey externalPlatformType);
			var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
													 BaseHelpers.GetClientProductBalance(client.Id, product?.Id ?? 0);

			response = JsonConvert.SerializeObject( new 
			{
				status = 1,
				user_balance = balance
			});

			switch (input.method)
			{
				case FiverscoolHelpers.Methods.UserBalance:
					break;
				//case BGGamesHelpers.Methods.PlaceBet:
				//	DoBet(input, client, clientSession, partnerProductSetting.Id, product.Id);
				//	break;
				//case BGGamesHelpers.Methods.AddWins:
				//	DoWin(input, client, clientSession, partnerProductSetting.Id, product);
				//	break;
				//case BGGamesHelpers.Methods.BetWin:
				//	DoBet(input, client, clientSession, partnerProductSetting.Id, product.Id);
				//	DoWin(input, client, clientSession, partnerProductSetting.Id, product);
				//	break;
			}
			var updatedBalance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
															BaseHelpers.GetClientProductBalance(client.Id, product.Id);
		}
		catch (FaultException<BllFnErrorType> ex)
		{
			if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
			{
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}
			var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
		}
		catch (Exception ex)
		{
			WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
			httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
		}
		WebApiApplication.DbLogger.Error($"Response: {response}");
		httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
		httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
		return httpResponseMessage;
	}
}