using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Hubs;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.BetShopWebApi.Controllers
{
	[Route("ProductsIntegration/[action]")]
	[ApiController]
	public class ProductsIntegrationController : ControllerBase
	{
		[HttpPost]
		public ApiResponseBase SendBetSelections(SendBetSelections betSelections)
		{
			try
			{
				var requestInput = new PlatformRequestBase
				{
					TimeZone = 0,
					LanguageId = Constants.DefaultLanguageId,
					PartnerId = Constants.MainPartnerId,
					Token = betSelections.Token
				};
				var resp = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetCashierSessionByToken);

				if (resp.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception($"Code: {resp.ResponseCode}, Description: {resp.Description}");
				var session = JsonConvert.DeserializeObject<AuthorizationOutput>(JsonConvert.SerializeObject(resp.ResponseObject));

				if (session.ResponseCode != Constants.SuccessResponseCode)
					return new ApiResponseBase
					{
						Description = PlatformIntegration.GetErrorById(new GetErrorInput
						{ ErrorId = Constants.Errors.SessionNotFound, LanguageId = requestInput.LanguageId }),
						ResponseCode = Constants.Errors.SessionNotFound
					};

				var cashier = BaseHub.ConnectedClients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					return new ApiResponseBase
					{
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}

				BaseHub.ConnectedClients.TryGetValue(session.Token, out CashierIdentity identity);
				if (identity == null)
					return new ApiResponseBase
					{
						Description = PlatformIntegration.GetErrorById(new GetErrorInput
						{ ErrorId = Constants.Errors.SessionNotFound, LanguageId = requestInput.LanguageId }),
						ResponseCode = Constants.Errors.SessionNotFound
					};

				foreach (var selection in betSelections.BetSelections)
				{
					selection.ProductId = Constants.GamesExternalIds.First(x => x.Value == selection.ProductId.ToString()).Key;
				}

				foreach (var cId in cashier.Value.ConnectionIds)
					BaseHub.CurrentContext.Clients.Client(cId).SendAsync("onSendBetSelections", betSelections);
				return new ApiResponseBase();
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + ex.StackTrace);
				var response = new ApiResponseBase
				{
					Description = ex.Message,
					ResponseCode = Constants.Errors.GeneralException
				};
				return response;
			}
		}

		[HttpPost]
		public ApiResponseBase CancelBetSelections(SendBetSelections betSelections)
		{
			try
			{
				var requestInput = new PlatformRequestBase
				{
					TimeZone = 0,
					LanguageId = Constants.DefaultLanguageId,
					PartnerId = Constants.MainPartnerId,
					Token = betSelections.Token
				};
				var resp = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetCashierSessionByToken);

				if (resp.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception($"Code: {resp.ResponseCode}, Description: {resp.Description}");
				var session = JsonConvert.DeserializeObject<AuthorizationOutput>(JsonConvert.SerializeObject(resp.ResponseObject));
				var cashier = BaseHub.ConnectedClients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					Log.Error("SocketNotFound " + betSelections.Token);
					return new ApiResponseBase
					{
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}
				if (!BaseHub.ConnectedClients.TryGetValue(session.Token, out CashierIdentity identity) || identity == null)
					return new ApiResponseBase
					{
						Description = PlatformIntegration.GetErrorById(new GetErrorInput
						{ ErrorId = Constants.Errors.SessionNotFound, LanguageId = Constants.DefaultLanguageId }),
						ResponseCode = Constants.Errors.SessionNotFound
					};

				foreach (var selection in betSelections.BetSelections)
				{
					selection.ProductId = Constants.GamesExternalIds.First(x => x.Value == selection.ProductId.ToString()).Key;
				}

				foreach (var cId in cashier.Value.ConnectionIds)
					BaseHub.CurrentContext.Clients.Client(cId).SendAsync("onCancelBetSelections", betSelections);
				return new ApiResponseBase();
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + ex.StackTrace);
				var response = new ApiResponseBase
				{
					Description = ex.Message,
					ResponseCode = Constants.Errors.GeneralException
				};
				return response;
			}
		}

		[HttpPost]
		public void PrintExternalProductTicket([FromQuery] RequestInfo info, PlaceBetOutput betOutput)
		{
			try
			{
				var requestInput = new PlatformRequestBase
				{
					TimeZone = 0,
					LanguageId = Constants.DefaultLanguageId,
					PartnerId = Constants.MainPartnerId,
					Token = betOutput.Token
				};
				var resp = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetCashierSessionByToken);

				if (resp.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception($"Code: {resp.ResponseCode}, Description: {resp.Description}");
				var session = JsonConvert.DeserializeObject<AuthorizationOutput>(JsonConvert.SerializeObject(resp.ResponseObject));
				var cashier = BaseHub.ConnectedClients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					Log.Error("SocketNotFound " + betOutput.Token);
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				}
				if (!BaseHub.ConnectedClients.TryGetValue(session.Token, out CashierIdentity identity) || identity == null)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				betOutput.Bets[0].BetDate = betOutput.Bets[0].BetDate.GetGMTDateFromUTC(identity.TimeZone);
				foreach (BllBetSelection selection in betOutput.Bets[0].BetSelections)
					selection.EventDate = selection.EventDate.GetGMTDateFromUTC(identity.TimeZone);
				foreach (var cId in cashier.Value.ConnectionIds)
					BaseHub.CurrentContext.Clients.Client(cId).SendAsync("onPrintExternalProductTicket", new ApiResponseBase { ResponseObject = betOutput });

			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + ex.StackTrace);
			}
		}
	}
}