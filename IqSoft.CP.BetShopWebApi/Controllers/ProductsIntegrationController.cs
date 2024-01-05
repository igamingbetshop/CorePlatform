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
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IqSoft.CP.BetShopWebApi.Controllers
{
	public class ProductsIntegrationController : ApiController
	{
		readonly IHubContext context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();

		[HttpPost]
		public SelectedNumbersOutput SendBetSelections(SendBetSelections betSelections)
		{
			try
			{
				var input = new GetCashierSessionInput
				{
					TimeZone = 0,
					LanguageId = Constants.DefaultLanguageId,
					PartnerId = Constants.MainPartnerId,
					SessionToken = betSelections.Token
				};
				var session = PlatformIntegration.GetCashierSessionByProductId(input);

				if (session.ResponseCode != Constants.SuccessResponseCode)
					return new SelectedNumbersOutput { Description = PlatformIntegration.GetErrorById(new GetErrorInput
					{ ErrorId = Constants.Errors.SessionNotFound, LanguageId = input.LanguageId }), ResponseCode = Constants.Errors.SessionNotFound };

				var cashier = WebApiApplication.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					return new SelectedNumbersOutput
					{
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}

				CashierIdentity identity;
				WebApiApplication.Clients.TryGetValue(session.Token, out identity);
				if (identity == null)
					return new SelectedNumbersOutput { Description = PlatformIntegration.GetErrorById(new GetErrorInput
					{ ErrorId = Constants.Errors.SessionNotFound, LanguageId = input.LanguageId }), ResponseCode = Constants.Errors.SessionNotFound };

				foreach (var selection in betSelections.BetSelections)
				{
					selection.ProductId = Constants.GamesExternalIds.First(x => x.Value == selection.ProductId.ToString()).Key;
				}

				var response = context.Clients.Clients(cashier.Value.ConnectionIds).onSendBetSelections(betSelections);
				return new SelectedNumbersOutput();
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
				var response = new SelectedNumbersOutput
				{
					Description = ex.Message,
					ResponseCode = Constants.Errors.GeneralException
				};
				return response;
			}
		}

		[HttpPost]
		public SelectedNumbersOutput CancelBetSelections(SendBetSelections betSelections)
		{
			try
			{
				var session = PlatformIntegration.GetCashierSessionByProductId(new GetCashierSessionInput
				{
					TimeZone = 0,
					LanguageId = Constants.DefaultLanguageId,
					PartnerId = Constants.MainPartnerId,
					SessionToken = betSelections.Token
				});
				if (session.ResponseCode != Constants.SuccessResponseCode)
					return new SelectedNumbersOutput { Description = PlatformIntegration.GetErrorById(new GetErrorInput
					{ ErrorId = Constants.Errors.SessionNotFound, LanguageId = Constants.DefaultLanguageId }), ResponseCode = Constants.Errors.SessionNotFound };

				var cashier = WebApiApplication.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					WebApiApplication.LogWriter.Error("SocketNotFound " + betSelections.Token);
					return new SelectedNumbersOutput
					{
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}

				CashierIdentity identity;
				WebApiApplication.Clients.TryGetValue(session.Token, out identity);
				if (identity == null)
					return new SelectedNumbersOutput { Description = PlatformIntegration.GetErrorById(new GetErrorInput
					{ ErrorId = Constants.Errors.SessionNotFound, LanguageId = Constants.DefaultLanguageId }), ResponseCode = Constants.Errors.SessionNotFound };

				foreach (var selection in betSelections.BetSelections)
				{
					selection.ProductId = Constants.GamesExternalIds.First(x => x.Value == selection.ProductId.ToString()).Key;
				}

				var response = context.Clients.Clients(cashier.Value.ConnectionIds).onCancelBetSelections(betSelections);
				return new SelectedNumbersOutput();
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
				var response = new SelectedNumbersOutput
				{
					Description = ex.Message,
					ResponseCode = Constants.Errors.GeneralException
				};
				return response;
			}
		}

		[HttpPost]
		public void PrintExternalProductTicket(PlaceBetOutput betOutput)
		{
			try
			{
				var session = PlatformIntegration.GetCashierSessionByProductId(new GetCashierSessionInput {
					TimeZone = 0,
					LanguageId = Constants.DefaultLanguageId,
					PartnerId = Constants.MainPartnerId,
					SessionToken = betOutput.Token
				});

				if (session.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());

				var cashier = WebApiApplication.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					WebApiApplication.LogWriter.Error("SocketNotFound " + betOutput.Token);
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				}
				CashierIdentity identity;
				WebApiApplication.Clients.TryGetValue(session.Token, out identity);
				if (identity == null)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				betOutput.Bets[0].BetDate = betOutput.Bets[0].BetDate.GetGMTDateFromUTC(identity.TimeZone);
				foreach (BllBetSelection selection in betOutput.Bets[0].BetSelections)
					selection.EventDate = selection.EventDate.GetGMTDateFromUTC(identity.TimeZone);
				var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
				context.Clients.Clients(cashier.Value.ConnectionIds).onPrintExternalProductTicket(new ClientRequestResponseBase { ResponseObject = betOutput });
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
			}
		}
	}
}
