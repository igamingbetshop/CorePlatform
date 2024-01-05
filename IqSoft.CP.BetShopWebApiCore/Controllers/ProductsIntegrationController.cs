using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Hubs;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApiCore;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.BetShopWebApi.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class ProductsIntegrationController : ControllerBase
	{

		[HttpPost]
		public SelectedNumbersOutput SendBetSelections(SendBetSelections betSelections)
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
					return new SelectedNumbersOutput { Description = Constants.Errors.SessionNotFound.ToString(), ResponseCode = Constants.Errors.SessionNotFound };

				var cashier = Program.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					Program.LogWriter.Error("SocketNotFound " + betSelections.Token);
					return new SelectedNumbersOutput
					{
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}

				CashierIdentity identity;
				Program.Clients.TryGetValue(session.Token, out identity);
				if (identity == null)
					return new SelectedNumbersOutput { Description = Constants.Errors.SessionNotFound.ToString(), ResponseCode = Constants.Errors.SessionNotFound };

				foreach (var selection in betSelections.BetSelections)
				{
					selection.ProductId = Constants.GamesExternalIds.First(x => x.Value == selection.ProductId.ToString()).Key;
				}

				var response = BaseHub.CurrentContext.Clients.Clients(cashier.Value.ConnectionIds).SendAsync("onSendBetSelections", betSelections);
				return new SelectedNumbersOutput();
			}
			catch (Exception ex)
			{
				Program.LogWriter.Error(ex);
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
					return new SelectedNumbersOutput { Description = Constants.Errors.SessionNotFound.ToString(), ResponseCode = Constants.Errors.SessionNotFound };

				var cashier = Program.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					Program.LogWriter.Error("SocketNotFound " + betSelections.Token);
					return new SelectedNumbersOutput
					{
						ResponseCode = Constants.Errors.SessionNotFound
					};
				}

				CashierIdentity identity;
				Program.Clients.TryGetValue(session.Token, out identity);
				if (identity == null)
					return new SelectedNumbersOutput { Description = Constants.Errors.SessionNotFound.ToString(), ResponseCode = Constants.Errors.SessionNotFound };

				foreach (var selection in betSelections.BetSelections)
				{
					selection.ProductId = Constants.GamesExternalIds.First(x => x.Value == selection.ProductId.ToString()).Key;
				}

				BaseHub.CurrentContext.Clients.Clients(cashier.Value.ConnectionIds).SendAsync("onCancelBetSelections", betSelections);
				return new SelectedNumbersOutput();
			}
			catch (Exception ex)
			{
				Program.LogWriter.Error(ex);
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
				var session = PlatformIntegration.GetCashierSessionByProductId(new GetCashierSessionInput
				{
					TimeZone = 0,
					LanguageId = Constants.DefaultLanguageId,
					PartnerId = Constants.MainPartnerId,
					SessionToken = betOutput.Token
				});

				if (session.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());

				var cashier = Program.Clients.FirstOrDefault(x => x.Key == session.Token);
				if (string.IsNullOrEmpty(session.Token) ||
					cashier.Equals(default(KeyValuePair<string, CashierIdentity>)))
				{
					Program.LogWriter.Error("SocketNotFound " + betOutput.Token);
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				}
				CashierIdentity identity;
				Program.Clients.TryGetValue(session.Token, out identity);
				if (identity == null)
					throw new Exception(Constants.Errors.SessionNotFound.ToString());
				betOutput.Bets[0].BetDate = betOutput.Bets[0].BetDate.GetGMTDateFromUTC(identity.TimeZone);
				foreach (BllBetSelection selection in betOutput.Bets[0].BetSelections)
					selection.EventDate = selection.EventDate.GetGMTDateFromUTC(identity.TimeZone);
				BaseHub.CurrentContext.Clients.Clients(cashier.Value.ConnectionIds).SendAsync("onPrintExternalProductTicket",
					new ClientRequestResponseBase { ResponseObject = betOutput });
			}
			catch (Exception ex)
			{
				Program.LogWriter.Error(ex);
			}
		}
	}
}
