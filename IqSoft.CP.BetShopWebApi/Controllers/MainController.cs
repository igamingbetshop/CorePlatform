using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Hubs;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApi.Models.Reports;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.BetShopWebApi.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "post")]
	public class MainController : ApiController
	{
		readonly IHubContext context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();	

		[HttpPost]
		public IHttpActionResult Authorization(AuthorizationInput request)
		{
            var requestInfo = CheckRequest();
            request.Ip = requestInfo.Ip;
            var resp = SendPlatformRequest(request, MethodBase.GetCurrentMethod().Name);
			if (resp.ResponseCode != 0)
				return Ok(resp);
			return Ok(JsonConvert.DeserializeObject<AuthorizationOutput>(JsonConvert.SerializeObject(resp.ResponseObject)));
		}

        [HttpPost]
        public IHttpActionResult CardReaderAuthorization(AuthorizationInput request)
        {
            var requestInfo = CheckRequest();
            request.Ip = requestInfo.Ip;
            var resp = SendPlatformRequest(request, MethodBase.GetCurrentMethod().Name);
            if (resp.ResponseCode != 0)
                return Ok(resp);
            return Ok(JsonConvert.DeserializeObject<ApiLoginResponse>(JsonConvert.SerializeObject(resp.ResponseObject)));
        }

        [HttpPost]
		public IHttpActionResult Login(AuthorizationInput request)
		{
            var requestInfo = CheckRequest();
            request.Ip = requestInfo.Ip;
			request.Country = requestInfo.Country;
            var resp = SendPlatformRequest(request, MethodBase.GetCurrentMethod().Name);
			if (resp.ResponseCode != 0)
				return Ok(resp);
			return Ok(JsonConvert.DeserializeObject<AuthorizationOutput>(JsonConvert.SerializeObject(resp.ResponseObject)));
		}

		[HttpPost]
		public ApiResponseBase ApiRequest(RequestBase requestInput)
		{
			var response = new ApiResponseBase();
            try
            {
                var requestInfo = CheckRequest();
                var client = WebApiApplication.Clients.FirstOrDefault(x => x.Key == requestInput.Token);
				switch (requestInput.Method)
				{
					case ApiMethods.PlaceBet:// To Be Deleted
						if (client.Equals(default(KeyValuePair<string, CashierIdentity>)))
							throw new Exception(Constants.Errors.SessionNotFound.ToString());
						var placeBetInput = JsonConvert.DeserializeObject<PlaceBetInput>(requestInput.RequestObject);
						response.ResponseObject = PlaceBet(requestInput.Token, placeBetInput, client.Value);
						break;
					case ApiMethods.Cashout: // To Be Deleted
						if (client.Equals(default(KeyValuePair<string, CashierIdentity>)))
							throw new Exception(Constants.Errors.SessionNotFound.ToString());
						var cashoutInput = JsonConvert.DeserializeObject<ApiCashoutInput>(requestInput.RequestObject);
						cashoutInput.Token = requestInput.Token;
						response.ResponseObject = Cashout(requestInput.Token, cashoutInput, client.Value);
						break;
					case ApiMethods.GetBetShopBets:
						response = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetBetShopBets);
						if (response.ResponseCode == Constants.SuccessResponseCode)
						{
							var betShopBetsOutput = JsonConvert.DeserializeObject<GetBetShopBetsOutput>(JsonConvert.SerializeObject(response.ResponseObject));
							betShopBetsOutput.Bets.Select(x => { x.Barcode = null; return x; });
							response.ResponseObject = betShopBetsOutput;
						}
						break;
					case ApiMethods.GetAvailableProducts:
						break;
                    case ApiMethods.GetBetByBarcode:
                        var getBetByBarcodeInput =
                            JsonConvert.DeserializeObject<GetBetByBarcodeInput>(requestInput.RequestObject);
                        getBetByBarcodeInput.Token = requestInput.Token;
                        getBetByBarcodeInput.CashDeskId = requestInput.CashDeskId;
                        getBetByBarcodeInput.LanguageId = requestInput.LanguageId;
                        getBetByBarcodeInput.PartnerId = requestInput.PartnerId;
                        getBetByBarcodeInput.TimeZone = requestInput.TimeZone;
                        response.ResponseObject = GetBetByBarcode(getBetByBarcodeInput);
                        break;
                    case ApiMethods.PlaceBetByBarcode: 
						var placeBetByBarcodeInput = JsonConvert.DeserializeObject<GetBetByBarcodeInput>(requestInput.RequestObject);
                        placeBetByBarcodeInput.Token = requestInput.Token;
                        placeBetByBarcodeInput.CashDeskId = requestInput.CashDeskId;
                        placeBetByBarcodeInput.LanguageId = requestInput.LanguageId;
                        placeBetByBarcodeInput.PartnerId = requestInput.PartnerId;
                        placeBetByBarcodeInput.TimeZone = requestInput.TimeZone;
                        response.ResponseObject = PlaceBetByBarcode(placeBetByBarcodeInput);
						break;
					case ApiMethods.GetTicketInfo:
						var getTicketInfoInput = JsonConvert.DeserializeObject<GetTicketInfoInput>(requestInput.RequestObject);
                        getTicketInfoInput.Token = requestInput.Token;
                       getTicketInfoInput.CashDeskId = requestInput.CashDeskId;
                       getTicketInfoInput.LanguageId = requestInput.LanguageId;
                       getTicketInfoInput.PartnerId = requestInput.PartnerId;
                       getTicketInfoInput.TimeZone = requestInput.TimeZone;
                        response.ResponseObject = GetTicketInfo(getTicketInfoInput);
						break;
					case ApiMethods.GetBetInfo:
						var getBetInfoInput = JsonConvert.DeserializeObject<GetTicketInfoInput>(requestInput.RequestObject);
                        getBetInfoInput.Token = requestInput.Token;
                        getBetInfoInput.CashDeskId = requestInput.CashDeskId;
                        getBetInfoInput.LanguageId = requestInput.LanguageId;
                        getBetInfoInput.PartnerId = requestInput.PartnerId;
                        getBetInfoInput.TimeZone = requestInput.TimeZone;
                        response.ResponseObject = GetBetInfo(getBetInfoInput);
						break;
					case ApiMethods.CancelBetSelection: //To Be Deleted
						if (client.Equals(default(KeyValuePair<string, CashierIdentity>)))
							throw new Exception(Constants.Errors.SessionNotFound.ToString());
						var platformCancelBetSelectionInput =
							JsonConvert.DeserializeObject<PlatformCancelBetSelectionInput>(requestInput.RequestObject);
						response.ResponseObject = CancelBetSelection(platformCancelBetSelectionInput, client.Value);
						break;
					case ApiMethods.GetCashDeskOperations:
						response = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetCashDeskOperations);
						if (response.ResponseCode == Constants.SuccessResponseCode)
						{
							var res = JsonConvert.DeserializeObject<GetCashDeskOperationsOutput>(JsonConvert.SerializeObject(response.ResponseObject));
							foreach (var op in res.Operations)
							{
								op.Id = op.TicketNumber ?? 0;
							}
							response.ResponseObject = res; 
						}
						break;                        
                    case ApiMethods.GetResultsReport:
                        var getResultsReportInput =
                            JsonConvert.DeserializeObject<GetResultsReportInput>(requestInput.RequestObject);
						getResultsReportInput.Token = requestInput.Token;
						getResultsReportInput.LanguageId = requestInput.LanguageId;
						getResultsReportInput.PartnerId = requestInput.PartnerId;
						getResultsReportInput.TimeZone = requestInput.TimeZone;
						response.ResponseObject = GetResultsReport(getResultsReportInput);
                        break;
                    case ApiMethods.GetUnitResult:
                        var getUnitResultInput = new GetUnitResultInput { Id = Convert.ToInt32(requestInput.RequestObject), Token = requestInput.Token };
                        response.ResponseObject = GetUnitResult(getUnitResultInput);
                        break;
                    default:
						return PlatformIntegration.SendRequestToPlatform(requestInput, requestInput.Method);
				}
			}
            catch (Exception e)
            {
                WebApiApplication.LogWriter.Error(e);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = e.Message;
            }
            return response;
        }

		private ApiResponseBase PlaceBet(string token, PlaceBetInput input, CashierIdentity identity)
		{
			var lockObject = new Object();
			var response = new PlaceBetOutput();
			Parallel.ForEach(input.Bets, betInput =>
			{
				var gameBetInput = new DoBetInput
				{
					PartnerId = identity.PartnerId,
					BetType = input.BetType,
					GameId = Constants.GamesExternalIds.First(x => x.Key == betInput.GameId).Value,
					ClientId = input.CashierId,
					CashDeskId = input.CashDeskId,
					Token = token,
					Amount = input.Amount,
					AcceptType = input.AcceptType,
					Info = input.Type.ToString(),
					SystemOutCount = input.SystemOutCount,
					Events = betInput.Events.Select(x => new DoBetInputItem
					{
						UnitId = x.UnitId,
						RoundId = x.RoundId,
						SelectionId = x.SelectionId,
						SelectionTypeId = x.SelectionTypeId,
						SelectionName = x.SelectionName,
						MarketTypeId = x.MarketTypeId,
						MarketId = x.MarketId,
						Coefficient = x.Coefficient,
						UnitName = x.UnitName,
						RoundName = x.RoundName,
						MarketName = x.MarketName,
						EventDate =
							(x.EventDate == null || x.EventDate == DateTime.MinValue ||
							 x.EventDate == DateTime.MaxValue)
								? DateTime.MinValue
								: x.EventDate.Value.GetUTCDateFromGmt(identity.TimeZone)
					}).ToList()
				};
				var betResponse = ProductsIntegration.DoBet(gameBetInput, betInput.GameId);

				foreach (var b in betResponse)
				{
					b.GameId = betInput.GameId;
				}

				lock (lockObject)
				{
					foreach (var bet in betResponse)
					{
						bet.BetDate = bet.BetDate.GetGMTDateFromUTC(identity.TimeZone);
						if (bet.BetSelections != null)
						{
							bool updateSelections = (bet.ResponseCode != Constants.SuccessResponseCode && bet.BetSelections.All(x => x.ResponseCode == Constants.SuccessResponseCode));
							foreach (var bs in bet.BetSelections)
							{
								bs.EventDate = bs.EventDate.GetGMTDateFromUTC(identity.TimeZone);
								if (updateSelections)
								{
									bs.ResponseCode = bet.ResponseCode;
									bs.Description = bet.Description;
								}
							}
						}
						response.Bets.Add(bet);
					}
				}
			});

			try
			{
				var inp = new PlatformRequestBase
                {
					CashDeskId = identity.CashDeskId,
					Token = token,
					TimeZone = identity.TimeZone,
					LanguageId = identity.LanguageId,
					PartnerId = identity.PartnerId
				};
                var r = PlatformIntegration.SendRequestToPlatform(inp, ApiMethods.GetCashDeskCurrentBalance);
				if (r.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception(JsonConvert.SerializeObject(r));
				var balance = JsonConvert.DeserializeObject<GetCashDeskCurrentBalanceOutput>(JsonConvert.SerializeObject(r.ResponseObject));
				response.Balance = balance == null ? 0 : balance.Balance;
				response.CurrentLimit = balance == null ? 0 : balance.CurrentLimit;
			}
			catch (Exception e)
			{
				WebApiApplication.LogWriter.Error(e);
			}

			var notAcceptedBet = response.Bets.FirstOrDefault(x => x.ResponseCode > 0);
			if(notAcceptedBet != null)
            {
				response.ResponseCode = notAcceptedBet.ResponseCode;
				response.Description = notAcceptedBet.Description;
            }

			return response;
		}
		private ApiResponseBase Cashout(string token, ApiCashoutInput input, CashierIdentity identity)
		{
			var requestInput = new GetBetByDocumentIdInput
			{
				DocumentId = input.BetDocumentId,
				LanguageId = identity.LanguageId,
				PartnerId = identity.PartnerId,
				TimeZone = identity.TimeZone,
				CashDeskId = identity.CashDeskId,
				Token = token,
				IsForPrint = false
			};
			var r = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetBetByDocumentId);
			if (r.ResponseCode != Constants.SuccessResponseCode)
				throw new Exception(JsonConvert.SerializeObject(r));
			var document = JsonConvert.DeserializeObject<GetBetByDocumentIdOutput>(JsonConvert.SerializeObject(r.ResponseObject));

			if (Int64.TryParse(document.ExternalId, out long betId))
				input.BetId = betId;

			var response = new PlaceBetOutput { ResponseObject = ProductsIntegration.Cashout(input) };
			try
			{
				var inp = new PlatformRequestBase
                {
					CashDeskId = identity.CashDeskId,
					Token = token,
					TimeZone = identity.TimeZone,
					LanguageId = identity.LanguageId,
					PartnerId = identity.PartnerId
				};
				r = PlatformIntegration.SendRequestToPlatform(inp, ApiMethods.GetCashDeskCurrentBalance);
				if (r.ResponseCode != Constants.SuccessResponseCode)
					throw new Exception(JsonConvert.SerializeObject(r));
				var balance = JsonConvert.DeserializeObject<GetCashDeskCurrentBalanceOutput>(JsonConvert.SerializeObject(r.ResponseObject));
				response.Balance = balance == null ? 0 : balance.Balance;
				response.CurrentLimit = balance == null ? 0 : balance.CurrentLimit;
			}
			catch (Exception e)
			{
				WebApiApplication.LogWriter.Error(e);
			}
			return response;
		}
		private ApiResponseBase PlaceBetByBarcode(GetBetByBarcodeInput input)
		{
            var r = PlatformIntegration.SendRequestToPlatform(input, ApiMethods.GetBetByBarcode);
			if (r.ResponseCode != Constants.SuccessResponseCode)
				throw new Exception(JsonConvert.SerializeObject(r));
			var bet = JsonConvert.DeserializeObject<GetBetShopBetsOutput>(JsonConvert.SerializeObject(r.ResponseObject));

			if (bet.Bets == null || !bet.Bets.Any())
				throw new Exception(Constants.Errors.TicketNotFound.ToString());
			if (bet.Bets[0].State != (int)BetDocumentStates.Uncalculated)
				throw new Exception(Constants.Errors.ProductNotFound.ToString());

			var ticket = ProductsIntegration.GetTicketInfo(new GetTicketInfoInput
			{
				Token = input.Token,
				TimeZone = input.TimeZone,
				LanguageId = input.LanguageId,
				PartnerId = input.PartnerId,
				TicketId = bet.Bets[0].BetDocumentId.ToString()
			}, bet.Bets[0].ProductId);
			var gameId = bet.Bets[0].ProductId;
			var placeBetInput = new PlaceBetInput
			{
				CashierId = input.CashierId,
				CashDeskId = input.CashDeskId,
				Amount = ticket.BetAmount,
				AcceptType = Enum.IsDefined(typeof(Constants.BetAcceptTypes), input.AcceptType) ? input.AcceptType : (int)Constants.BetAcceptTypes.None,
				BetType = ticket.TypeId,
				Type = Convert.ToInt32(ticket.Info),
				Bets = new List<PlaceBetInputItem>
				{
					new PlaceBetInputItem
					{
						GameId = gameId,
						Events = ticket.BetSelections.Select(x => new PlaceBetInputItemElement
						{
							UnitId = x.UnitId,
							RoundId = x.RoundId,
							MarketTypeId = x.MarketTypeId,
							MarketId = x.MarketId,
							SelectionId = x.SelectionId,
							SelectionTypeId = x.SelectionTypeId,
							SelectionName = x.SelectionName,
							Coefficient = x.Coefficient
						}).ToList()
					}
				}
			};
			return PlaceBet(input.Token, placeBetInput, new CashierIdentity
			{
				TimeZone = input.TimeZone ?? 0,
				LanguageId = input.LanguageId,
				PartnerId = input.PartnerId,
				CashierId = input.CashierId,
				CashDeskId = input.CashDeskId
			});
		}
		private ApiResponseBase GetTicketInfo(GetTicketInfoInput input)
		{
            var requestInput = new GetBetByDocumentIdInput
            {
                DocumentId = Convert.ToInt64(input.TicketId),
                LanguageId = input.LanguageId,
                PartnerId = input.PartnerId,
                TimeZone = input.TimeZone,
                CashDeskId = input.CashDeskId,
                Token = input.Token,
                IsForPrint = true
            };
            var r = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetBetByDocumentId);
            if (r.ResponseCode != Constants.SuccessResponseCode)
                throw new Exception(JsonConvert.SerializeObject(r));
            var bet = JsonConvert.DeserializeObject<GetBetByDocumentIdOutput>(JsonConvert.SerializeObject(r.ResponseObject));
			if (bet == null || bet.Id == 0)
				return new ApiResponseBase { Description = PlatformIntegration.GetErrorById(new GetErrorInput 
					{ ErrorId = Constants.Errors.TicketNotFound, LanguageId = input.LanguageId }), ResponseCode = Constants.Errors.TicketNotFound };
            var now = DateTime.UtcNow;
            if (bet.NumberOfPrints > 3 || (now - bet.CreationTime).TotalMinutes > 30)
				return new ApiResponseBase { Description = PlatformIntegration.GetErrorById(new GetErrorInput
					{ ErrorId = Constants.Errors.CanNotPrintTicket, LanguageId = input.LanguageId }), ResponseCode = Constants.Errors.CanNotPrintTicket };

			var response = ProductsIntegration.GetTicketInfo(input, bet.GameId);
			if (response == null) return null;

			response.BetDate = response.BetDate.GetGMTDateFromUTC(input.TimeZone ?? 0);
			if (response.BetSelections != null)
			{
				foreach (var bs in response.BetSelections)
				{
					bs.EventDate = bs.EventDate.GetGMTDateFromUTC(input.TimeZone ?? 0);
				}
			}
			return response;
		}
		private object GetBetByBarcode(GetBetByBarcodeInput input)
		{
           var resp = PlatformIntegration.SendRequestToPlatform(input, ApiMethods.GetBetByBarcode);
			if (resp.ResponseCode != Constants.SuccessResponseCode)
				return resp;
			
				var betShopBetsOutput = JsonConvert.DeserializeObject<GetBetShopBetsOutput>(JsonConvert.SerializeObject(resp.ResponseObject));
				if (betShopBetsOutput.Bets == null || !betShopBetsOutput.Bets.Any())
					throw new Exception($"Code: {Constants.Errors.TicketNotFound}, Description: {nameof(Constants.Errors.TicketNotFound)}");

			var response = ProductsIntegration.GetTicketInfo(new GetTicketInfoInput
			{
				Token = input.Token,
				TimeZone = input.TimeZone,
				LanguageId = input.LanguageId,
				PartnerId = input.PartnerId,
				TicketId = betShopBetsOutput.Bets[0].BetDocumentId.ToString()
			}, betShopBetsOutput.Bets[0].ProductId);
			
			if (response != null)
			{
				response.BetDate = response.BetDate.GetGMTDateFromUTC(input.TimeZone ?? 0);
				if (response.BetSelections != null)
				{
					foreach (var bs in response.BetSelections)
					{
						bs.EventDate = bs.EventDate.GetGMTDateFromUTC(input.TimeZone ?? 0);
					}
				}
                betShopBetsOutput.Bets[0].Coefficient = response.Coefficient;
				betShopBetsOutput.Bets[0].BetSelections = response.BetSelections;
				betShopBetsOutput.Bets[0].NumberOfMatches = response.NumberOfMatches;
				betShopBetsOutput.Bets[0].NumberOfBets = response.NumberOfBets;
				betShopBetsOutput.Bets[0].AmountPerBet = response.AmountPerBet;
				betShopBetsOutput.Bets[0].CommissionFee = response.CommissionFee;
				betShopBetsOutput.Bets[0].PossibleWin = response.PossibleWin;
				betShopBetsOutput.Bets[0].Barcode = input.Barcode;
				betShopBetsOutput.Bets[0].SystemOutCounts = response.SystemOutCounts;
				betShopBetsOutput.Bets[0].CashoutAmount = response.CashoutAmount;
				betShopBetsOutput.Bets[0].BlockedForCashout = response.BlockedForCashout;
			}
			return betShopBetsOutput.Bets[0];
		}
		private ApiResponseBase GetBetInfo(GetTicketInfoInput input)
		{
            var requestInput = new GetBetByDocumentIdInput
            {
                Token = input.Token,
                DocumentId = Convert.ToInt64(input.TicketId),
                TimeZone = input.TimeZone,
                LanguageId = input.LanguageId,
                PartnerId = input.PartnerId,
                CashDeskId = input.CashDeskId,
                IsForPrint = false
            };
            var r = PlatformIntegration.SendRequestToPlatform(requestInput, ApiMethods.GetBetByDocumentId);
            if (r.ResponseCode != Constants.SuccessResponseCode)
                throw new Exception(JsonConvert.SerializeObject(r));
            var bet = JsonConvert.DeserializeObject<GetBetByDocumentIdOutput>(JsonConvert.SerializeObject(r.ResponseObject));
			if (bet == null || bet.Id == 0)
				throw new Exception(Constants.Errors.TicketNotFound.ToString());
			var response = ProductsIntegration.GetTicketInfo(input, bet.GameId);
			if (response == null) return null;

			WebApiApplication.LogWriter.Info(JsonConvert.SerializeObject(response));
			response.BetDate = response.BetDate.GetGMTDateFromUTC(input.TimeZone ?? 0);
			response.Barcode = 0;
			if (response.BetSelections != null)
			{
				foreach (var bs in response.BetSelections)
				{
					bs.EventDate = bs.EventDate.GetGMTDateFromUTC(input.TimeZone ?? 0);
				}
			}

			return response;
		}
		private ApiResponseBase CancelBetSelection(PlatformCancelBetSelectionInput input, CashierIdentity identity)
		{
			var cancelInput = new CancelBetSelectionInput
			{
				GameId = Constants.GamesExternalIds.First(x => x.Key == input.GameId).Value,
				Token = input.Token,
				Selections = new List<CancelBetSelectionInputItem>
				{
					new CancelBetSelectionInputItem
					{
						SelectionTypeId = input.SelectionTypeId,
						SelectionId = input.SelectionId,
						GameUnitId = input.GameUnitId
					}
				}
			};
			var cancelResponse = (CancelBetSelectionOutput)ProductsIntegration.CancelBetSelection(cancelInput, input.GameId);
			cancelResponse.MarketTypeId = input.MarketTypeId;
			cancelResponse.SelectionTypeId = input.SelectionTypeId;
			cancelResponse.SelectionId = input.SelectionId;
			cancelResponse.Token = input.Token;
			cancelResponse.GameId = input.GameId;
			cancelResponse.GameUnitId = input.GameUnitId;
			context.Clients.Clients(identity.ConnectionIds).onCancelBetSelections(new
			{
				Token = input.Token,
				BetSelections = new List<object>
				{
					new
					{
						ProductId = input.GameId,
						SelectionTypeId = input.SelectionTypeId,
						SelectionId = input.SelectionId,
						GameUnitId = input.GameUnitId
					}
				}
			});

			return cancelResponse;
		}
        private RequestInfo CheckRequest()
        {
            var requestInfo = new RequestInfo();
            requestInfo.Ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
            if (string.IsNullOrEmpty(requestInfo.Ip))
                requestInfo.Ip = HttpContext.Current.Request.UserHostAddress;
            requestInfo.Country = HttpContext.Current.Request.Headers.Get("CF-IPCountry");

            WebApiApplication.LogWriter.Info(JsonConvert.SerializeObject(requestInfo));
            if (WebApiApplication.BlockedIps.Contains(requestInfo.Ip))
                throw new Exception(Constants.Errors.DontHavePermission.ToString());
            if (!WebApiApplication.WhitelistedIps.Contains(requestInfo.Ip) && !WebApiApplication.WhitelistedCountries.Contains(requestInfo.Country))
                throw new Exception(Constants.Errors.DontHavePermission.ToString());
            return requestInfo;
        }

        private ApiResponseBase SendPlatformRequest<T>(T requestInput, string method)
        {
            try
            {
                return PlatformIntegration.SendRequestToPlatform(requestInput, method);
            }
            catch (Exception ex)
            {
                WebApiApplication.LogWriter.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        #region Reports

        private ApiResponseBase GetResultsReport(GetResultsReportInput input)
        {
            input.GameId = Convert.ToInt32(Constants.GamesExternalIds.First(x => x.Key == input.GameId).Value);
            return ProductsIntegration.GetResultsReport(input);
        }
        private ApiResponseBase GetUnitResult(GetUnitResultInput input)
        {
            return ProductsIntegration.GetUnitResult(input);
        }	
		
        #endregion
    }
}