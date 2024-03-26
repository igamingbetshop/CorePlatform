using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Hubs;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace IqSoft.CP.BetShopWebApi.Controllers
{
	public class ProductsIntegrationController : ApiController
	{
		readonly IHubContext context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();

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

				var cashier = WebApiApplication.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					return new ApiResponseBase
                    {
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}

				WebApiApplication.Clients.TryGetValue(session.Token, out CashierIdentity identity);
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

				var response = context.Clients.Clients(cashier.Value.ConnectionIds).onSendBetSelections(betSelections);
				return new ApiResponseBase();
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
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
                var cashier = WebApiApplication.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					WebApiApplication.LogWriter.Error("SocketNotFound " + betSelections.Token);
					return new ApiResponseBase
                    {
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}
				CashierIdentity identity;
				if(!WebApiApplication.Clients.TryGetValue(session.Token, out identity) || identity == null)
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

				var response = context.Clients.Clients(cashier.Value.ConnectionIds).onCancelBetSelections(betSelections);
				return new ApiResponseBase();
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
				var response = new ApiResponseBase
                {
					Description = ex.Message,
					ResponseCode = Constants.Errors.GeneralException
				};
				return response;
			}
		}

		[HttpPost]
		public void PrintExternalProductTicket([FromUri] RequestInfo info, PlaceBetOutput betOutput)
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
				var cashier = WebApiApplication.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					WebApiApplication.LogWriter.Error("SocketNotFound " + betOutput.Token);
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				}
				if (!WebApiApplication.Clients.TryGetValue(session.Token, out CashierIdentity identity) || identity == null)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				betOutput.Bets[0].BetDate = betOutput.Bets[0].BetDate.GetGMTDateFromUTC(identity.TimeZone);
				foreach (BllBetSelection selection in betOutput.Bets[0].BetSelections)
					selection.EventDate = selection.EventDate.GetGMTDateFromUTC(identity.TimeZone);
				var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
				context.Clients.Clients(cashier.Value.ConnectionIds).onPrintExternalProductTicket(new ApiResponseBase { ResponseObject = betOutput });
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
			}
		}
	}
}