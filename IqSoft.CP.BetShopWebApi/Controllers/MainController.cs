using IqSoft.CP.BetShopGatewayWebApi.Common;
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
		public IHttpActionResult CardReaderAuthorization([FromUri] RequestInfo info, AuthorizationInput request)
		{
			try
			{
				var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
				var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
				if (WebApiApplication.BlockedIps.Contains(ip))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());
				if (!WebApiApplication.WhitelistedIps.Contains(ip) && !WebApiApplication.WhitelistedCountries.Contains(ipCountry))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());

				request.Ip = ip;
				var response = PlatformIntegration.CardReaderAuthorization(request);

				var resp = new
				{
					ResponseCode = response.ResponseCode.ToString(),
					Description = response.Description,
					Token = response.Token
				};
				return Ok(resp);
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
				var response = new
				{
					ResponseCode = Constants.Errors.GeneralException.ToString(),
					Description = ex.Message
				};
				return Ok(response);
			}
		}
		[HttpPost]
		public AuthorizationOutput Authorization(AuthorizationInput request)
		{
			try
			{
				var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
				var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
				if (WebApiApplication.BlockedIps.Contains(ip))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());
				if (!WebApiApplication.WhitelistedIps.Contains(ip) && !WebApiApplication.WhitelistedCountries.Contains(ipCountry))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());

				request.Ip = ip;
				var response = PlatformIntegration.Authorization(request);
				return response;
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
				var response = new AuthorizationOutput
				{
					ResponseCode = Constants.Errors.GeneralException,
					Description = ex.Message
				};
				return response;
			}
		}
        [HttpPost]
		public AuthorizationOutput Login(AuthorizationInput request)
		{
			try
			{
				var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
				var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
				if (WebApiApplication.BlockedIps.Contains(ip))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());
				if (!WebApiApplication.WhitelistedIps.Contains(ip) && !WebApiApplication.WhitelistedCountries.Contains(ipCountry))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());

				request.Ip = ip;
				var response = PlatformIntegration.Login(request);
				return response;
			}
			catch (Exception ex)
			{
				WebApiApplication.LogWriter.Error(ex);
				var response = new AuthorizationOutput
				{
					ResponseCode = Constants.Errors.GeneralException,
					Description = ex.Message
				};
				return response;
			}
		}
		[HttpPost]
		public ClientRequestResponseBase ApiRequest(RequestBase request)
		{
			var response = new ClientRequestResponseBase { };

			try
			{
				var ipCountry = HttpContext.Current.Request.Headers.Get("CF-IPCountry");
				var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
				if (WebApiApplication.BlockedIps.Contains(ip))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());
				if (!WebApiApplication.WhitelistedIps.Contains(ip) && !WebApiApplication.WhitelistedCountries.Contains(ipCountry))
					throw new Exception(Constants.Errors.DontHavePermission.ToString());

				var client = WebApiApplication.Clients.FirstOrDefault(x => x.Key == request.Token);
				switch (request.Method)
				{
					case ApiMethods.LogoutUser:
						var logoutUserInput = JsonConvert.DeserializeObject<CloseSessionInput>(request.RequestObject);
						logoutUserInput.Token = request.Token;
						logoutUserInput.CashDeskId = request.CashDeskId;
						logoutUserInput.LanguageId = request.LanguageId;
						logoutUserInput.PartnerId = request.PartnerId;
						logoutUserInput.TimeZone = request.TimeZone;
						response.ResponseObject = LogoutUser(logoutUserInput);
						break;
					case ApiMethods.ChangeCashierPassword:
						var changePasswordInput = JsonConvert.DeserializeObject<ChangePasswordInput>(request.RequestObject);
						changePasswordInput.Token = request.Token;
						changePasswordInput.CashDeskId = request.CashDeskId;
						changePasswordInput.LanguageId = request.LanguageId;
						changePasswordInput.PartnerId = request.PartnerId;
						changePasswordInput.TimeZone = request.TimeZone;
						response.ResponseObject = ChangeCashierPassword(changePasswordInput);
						break;
					case ApiMethods.AssignPin:
						var assignPinInput = JsonConvert.DeserializeObject<AssignPinInput>(request.RequestObject);
						assignPinInput.Token = request.Token;
						assignPinInput.CashDeskId = request.CashDeskId;
						assignPinInput.LanguageId = request.LanguageId;
						assignPinInput.PartnerId = request.PartnerId;
						assignPinInput.TimeZone = request.TimeZone;
						response.ResponseObject = AssignPin(assignPinInput);
						break;

					case ApiMethods.GetProductSession:
						var getProductSessionInput =
							JsonConvert.DeserializeObject<GetProductSessionInput>(request.RequestObject);
						getProductSessionInput.Token = request.Token;
						getProductSessionInput.CashDeskId = request.CashDeskId;
						getProductSessionInput.LanguageId = request.LanguageId;
						getProductSessionInput.PartnerId = request.PartnerId;
						getProductSessionInput.TimeZone = request.TimeZone;
						response.ResponseObject = PlatformIntegration.GetProductSession(getProductSessionInput);
						break;
					case ApiMethods.PlaceBet: // To Be Deleted
						if (client.Equals(default(KeyValuePair<string, CashierIdentity>)))
							throw new Exception(Constants.Errors.SessionNotFound.ToString());
						var placeBetInput = JsonConvert.DeserializeObject<PlaceBetInput>(request.RequestObject);
						response.ResponseObject = PlaceBet(request.Token, placeBetInput, client.Value);
						break;
					case ApiMethods.Cashout: // To Be Deleted
						if (client.Equals(default(KeyValuePair<string, CashierIdentity>)))
							throw new Exception(Constants.Errors.SessionNotFound.ToString());
						var cashoutInput = JsonConvert.DeserializeObject<ApiCashoutInput>(request.RequestObject);
						cashoutInput.Token = request.Token;
						response.ResponseObject = Cashout(request.Token, cashoutInput, client.Value);
						break;
					case ApiMethods.GetClients:
						var getClientInput = JsonConvert.DeserializeObject<GetClientInput>(request.RequestObject);
						getClientInput.Token = request.Token;
						getClientInput.CashDeskId = request.CashDeskId;
						getClientInput.LanguageId = request.LanguageId;
						getClientInput.PartnerId = request.PartnerId;
						getClientInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetClients(getClientInput);
						break;
					case ApiMethods.GetCashDesks:
						var getCashDesks = JsonConvert.DeserializeObject<ApiFilterCashDesk>(request.RequestObject);
						getCashDesks.Token = request.Token;
						getCashDesks.CashDeskId = request.CashDeskId;
						getCashDesks.LanguageId = request.LanguageId;
						getCashDesks.PartnerId = request.PartnerId;
						getCashDesks.TimeZone = request.TimeZone;
						response.ResponseObject = GetCashDesks(getCashDesks);
						break;
					case ApiMethods.GetClient:
						getClientInput = JsonConvert.DeserializeObject<GetClientInput>(request.RequestObject);
						getClientInput.Token = request.Token;
						getClientInput.CashDeskId = request.CashDeskId;
						getClientInput.LanguageId = request.LanguageId;
						getClientInput.PartnerId = request.PartnerId;
						getClientInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetClient(getClientInput);
						break;
					case ApiMethods.ResetClientPassword:
						var resetPasswordtInput = JsonConvert.DeserializeObject<GetClientInput>(request.RequestObject);
						resetPasswordtInput.Token = request.Token;
						resetPasswordtInput.CashDeskId = request.CashDeskId;
						resetPasswordtInput.LanguageId = request.LanguageId;
						response.ResponseObject = ResetClientPassword(resetPasswordtInput);
						break;
					case ApiMethods.EditClient:
						var editClientInput = JsonConvert.DeserializeObject<ClientModel>(request.RequestObject);
						editClientInput.Token = request.Token;
						editClientInput.CashDeskId = request.CashDeskId;
						editClientInput.LanguageId = request.LanguageId;
						editClientInput.PartnerId = request.PartnerId;
						editClientInput.TimeZone = request.TimeZone;
						response.ResponseObject = EditClient(editClientInput);
						break;
					case ApiMethods.RegisterClient:
						var clientInput = JsonConvert.DeserializeObject<ClientModel>(request.RequestObject);
						clientInput.Token = request.Token;
						clientInput.CashDeskId = request.CashDeskId;
						clientInput.LanguageId = request.LanguageId;
						clientInput.PartnerId = request.PartnerId;
						clientInput.TimeZone = request.TimeZone;
						response.ResponseObject = RegisterClient(clientInput);
						break;
					case ApiMethods.DepositToInternetClient:
						var depositToInternetClientInput =
							JsonConvert.DeserializeObject<DepositToInternetClientInput>(request.RequestObject);
						depositToInternetClientInput.Token = request.Token;
						depositToInternetClientInput.CashDeskId = request.CashDeskId;
						depositToInternetClientInput.LanguageId = request.LanguageId;
						depositToInternetClientInput.PartnerId = request.PartnerId;
						depositToInternetClientInput.TimeZone = request.TimeZone;
						response.ResponseObject = DepositToInternetClient(depositToInternetClientInput);
						break;
					case ApiMethods.GetPaymentRequests:
						var getPaymentRequestsInput =
							JsonConvert.DeserializeObject<GetPaymentRequestsInput>(request.RequestObject);
						getPaymentRequestsInput.Token = request.Token;
						getPaymentRequestsInput.CashDeskId = request.CashDeskId;
						getPaymentRequestsInput.LanguageId = request.LanguageId;
						getPaymentRequestsInput.PartnerId = request.PartnerId;
						getPaymentRequestsInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetPaymentRequests(getPaymentRequestsInput);
						break;
					case ApiMethods.PayPaymentRequest:
						var payPaymentRequestInput =
							JsonConvert.DeserializeObject<PayPaymentRequestInput>(request.RequestObject);
						payPaymentRequestInput.Token = request.Token;
						payPaymentRequestInput.CashDeskId = request.CashDeskId;
						payPaymentRequestInput.LanguageId = request.LanguageId;
						payPaymentRequestInput.PartnerId = request.PartnerId;
						payPaymentRequestInput.TimeZone = request.TimeZone;
						response.ResponseObject = PayPaymentRequest(payPaymentRequestInput);
						break;
					case ApiMethods.GetBetShopBets:
						var getBetShopBetsInput =
							JsonConvert.DeserializeObject<GetBetShopBetsInput>(request.RequestObject);
						getBetShopBetsInput.Token = request.Token;
						getBetShopBetsInput.CashDeskId = request.CashDeskId;
						getBetShopBetsInput.LanguageId = request.LanguageId;
						getBetShopBetsInput.PartnerId = request.PartnerId;
						getBetShopBetsInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetBetShopBets(getBetShopBetsInput);
						break;
					case ApiMethods.GetBetShopOperations:
						var getOperationsInput = JsonConvert.DeserializeObject<GetOperationsInput>(request.RequestObject);
						getOperationsInput.Token = request.Token;
						getOperationsInput.CashDeskId = request.CashDeskId;
						getOperationsInput.LanguageId = request.LanguageId;
						getOperationsInput.PartnerId = request.PartnerId;
						getOperationsInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetOperations(getOperationsInput);
						break;
					case ApiMethods.PayWin:
						var payWinInput = JsonConvert.DeserializeObject<PayWinInput>(request.RequestObject);
						payWinInput.Token = request.Token;
						payWinInput.CashDeskId = request.CashDeskId;
						payWinInput.LanguageId = request.LanguageId;
						payWinInput.PartnerId = request.PartnerId;
						payWinInput.TimeZone = request.TimeZone;
						response.ResponseObject = PayWin(payWinInput);
						break;
					case ApiMethods.GetBetByBarcode:
						var getBetByBarcodeInput =
							JsonConvert.DeserializeObject<GetBetByBarcodeInput>(request.RequestObject);
						getBetByBarcodeInput.Token = request.Token;
						getBetByBarcodeInput.CashDeskId = request.CashDeskId;
						getBetByBarcodeInput.LanguageId = request.LanguageId;
						getBetByBarcodeInput.PartnerId = request.PartnerId;
						getBetByBarcodeInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetBetByBarcode(getBetByBarcodeInput);
						break;
					case ApiMethods.PlaceBetByBarcode:
						var placeBetByBarcodeInput =
							JsonConvert.DeserializeObject<GetBetByBarcodeInput>(request.RequestObject);
						placeBetByBarcodeInput.Token = request.Token;
						placeBetByBarcodeInput.CashDeskId = request.CashDeskId;
						placeBetByBarcodeInput.LanguageId = request.LanguageId;
						placeBetByBarcodeInput.PartnerId = request.PartnerId;
						placeBetByBarcodeInput.TimeZone = request.TimeZone;
						response.ResponseObject = PlaceBetByBarcode(placeBetByBarcodeInput);
						break;
					case ApiMethods.GetTicketInfo:
						var getTicketInfoInput = JsonConvert.DeserializeObject<GetTicketInfoInput>(request.RequestObject);
						getTicketInfoInput.Token = request.Token;
						getTicketInfoInput.CashDeskId = request.CashDeskId;
						getTicketInfoInput.LanguageId = request.LanguageId;
						getTicketInfoInput.PartnerId = request.PartnerId;
						getTicketInfoInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetTicketInfo(getTicketInfoInput);
						break;
					case ApiMethods.GetBetInfo:
						var getBetInfoInput = JsonConvert.DeserializeObject<GetTicketInfoInput>(request.RequestObject);
						getBetInfoInput.Token = request.Token;
						getBetInfoInput.CashDeskId = request.CashDeskId;
						getBetInfoInput.LanguageId = request.LanguageId;
						getBetInfoInput.PartnerId = request.PartnerId;
						getBetInfoInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetBetInfo(getBetInfoInput);
						break;
					case ApiMethods.CancelBetSelection: //To Be Deleted
						if (client.Equals(default(KeyValuePair<string, CashierIdentity>)))
							throw new Exception(Constants.Errors.SessionNotFound.ToString());
						var platformCancelBetSelectionInput =
							JsonConvert.DeserializeObject<PlatformCancelBetSelectionInput>(request.RequestObject);
						response.ResponseObject = CancelBetSelection(platformCancelBetSelectionInput, client.Value);
						break;
					case ApiMethods.GetCashDeskInfo:
						var getCashDeskInfoInput =
							JsonConvert.DeserializeObject<GetCashDeskInfoInput>(request.RequestObject);
						getCashDeskInfoInput.Token = request.Token;
						getCashDeskInfoInput.CashDeskId = request.CashDeskId;
						getCashDeskInfoInput.LanguageId = request.LanguageId;
						getCashDeskInfoInput.PartnerId = request.PartnerId;
						getCashDeskInfoInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetCashDeskInfo(getCashDeskInfoInput);
						break;
					case ApiMethods.GetShiftReport:
						var getShiftReportInput =
							JsonConvert.DeserializeObject<GetShiftReportInput>(request.RequestObject);
						getShiftReportInput.Token = request.Token;
						getShiftReportInput.CashDeskId = request.CashDeskId;
						getShiftReportInput.LanguageId = request.LanguageId;
						getShiftReportInput.PartnerId = request.PartnerId;
						getShiftReportInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetShiftReport(getShiftReportInput);
						break;
					case ApiMethods.GetCashDeskOperations:
						var getCashDeskOperationsInput =
							JsonConvert.DeserializeObject<GetCashDeskOperationsInput>(request.RequestObject);
						getCashDeskOperationsInput.Token = request.Token;
						getCashDeskOperationsInput.CashDeskId = request.CashDeskId;
						getCashDeskOperationsInput.LanguageId = request.LanguageId;
						getCashDeskOperationsInput.PartnerId = request.PartnerId;
						getCashDeskOperationsInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetCashDeskOperations(getCashDeskOperationsInput);
						break;
					case ApiMethods.CloseShift:
						var closeShiftInput = JsonConvert.DeserializeObject<CloseShiftInput>(request.RequestObject);
						closeShiftInput.Token = request.Token;
						closeShiftInput.CashDeskId = request.CashDeskId;
						closeShiftInput.LanguageId = request.LanguageId;
						closeShiftInput.PartnerId = request.PartnerId;
						closeShiftInput.TimeZone = request.TimeZone;
						response.ResponseObject = CloseShift(closeShiftInput);
						break;
					case ApiMethods.GetAvailableProducts:
						response.ResponseObject = GetAvailableProducts(request.Token);
						break;
					case ApiMethods.GetBalance:
						var getBalanceInput = new GetCashDeskCurrentBalanceIntput();
						getBalanceInput.Token = request.Token;
						getBalanceInput.CashDeskId = request.CashDeskId;
						getBalanceInput.LanguageId = request.LanguageId;
						getBalanceInput.PartnerId = request.PartnerId;
						getBalanceInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetBalance(getBalanceInput);
						break;
					case ApiMethods.GetResultsReport:
						var getResultsReportInput =
							JsonConvert.DeserializeObject<GetResultsReportInput>(request.RequestObject);
						getResultsReportInput.Token = request.Token;
						getResultsReportInput.LanguageId = request.LanguageId;
						getResultsReportInput.PartnerId = request.PartnerId;
						getResultsReportInput.TimeZone = request.TimeZone;
						response.ResponseObject = GetResultsReport(getResultsReportInput);
						break;
					case ApiMethods.GetUnitResult:
						var getUnitResultInput = new GetUnitResultInput { Id = Convert.ToInt32(request.RequestObject), Token = request.Token };
						response.ResponseObject = GetUnitResult(getUnitResultInput);
						break;
					case ApiMethods.DepositToTerminal:
						var depositToTerminalInput = JsonConvert.DeserializeObject<DepositToInternetClientInput>(request.RequestObject);
						depositToTerminalInput.Token = request.Token;
						depositToTerminalInput.CashDeskId = request.CashDeskId;
						depositToTerminalInput.LanguageId = request.LanguageId;
						depositToTerminalInput.PartnerId = request.PartnerId;
						depositToTerminalInput.TimeZone = request.TimeZone;
						response.ResponseObject = DepositToTerminal(depositToTerminalInput);
						break;
					case ApiMethods.WithdrawTerminalFunds:
						var withdrawTerminalFundsInput = new PayWinInput { CashDeskId = Convert.ToInt32(request.RequestObject), Token = request.Token };
						response.ResponseObject = WithdrawTerminalFunds(withdrawTerminalFundsInput);
						break;
                    case ApiMethods.GetRegions:
                        response.ResponseObject =GetRegions(JsonConvert.DeserializeObject<ApiRegionInput>(request.RequestObject));
                        break;
					case ApiMethods.CreateWithdrawPaymentRequest:
						var createPaymentRequest = JsonConvert.DeserializeObject<CreatePaymentRequest>(request.RequestObject);
						createPaymentRequest.Token = request.Token;
						createPaymentRequest.CashDeskId = request.CashDeskId;
						createPaymentRequest.LanguageId = request.LanguageId;
						createPaymentRequest.PartnerId = request.PartnerId;
						createPaymentRequest.TimeZone = request.TimeZone;
						response.ResponseObject = CreateWithdrawPaymentRequest(createPaymentRequest);
						break;
					default:
						response.ResponseObject = new ClientRequestResponseBase
						{
							ResponseCode = Constants.Errors.MethodNotFound
						};
						break;
				}
				var rsp = (ClientRequestResponseBase)response.ResponseObject;
				response.ResponseCode = rsp.ResponseCode;
				response.Description = rsp.Description;
			}
			catch (Exception e)
			{
				WebApiApplication.LogWriter.Error(e);
				response.ResponseCode = Constants.Errors.GeneralException;
				response.Description = e.Message;
			}
			return response;
		}
		
		private ClientRequestResponseBase GetRegions(ApiRegionInput request)
		{
			return PlatformIntegration.GetRegions(request);
		}
		private ClientRequestResponseBase GetProductSession(GetProductSessionInput input)
		{
			var response = PlatformIntegration.GetProductSession(input);
			return response;
		}
		private ClientRequestResponseBase LogoutUser(CloseSessionInput input)
		{
			return PlatformIntegration.LogoutUser(input);
		}
		private ClientRequestResponseBase ChangeCashierPassword(ChangePasswordInput changePasswordInput)
		{
			return PlatformIntegration.ChangeCashierPassword(changePasswordInput);
		}

		private ClientRequestResponseBase AssignPin(AssignPinInput assignPinInput)
		{
			return PlatformIntegration.AssignPin(assignPinInput);
		}
		private ClientRequestResponseBase PlaceBet(string token, PlaceBetInput input, CashierIdentity identity)
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
				var balance = GetBalance(new GetCashDeskCurrentBalanceIntput
				{
					CashDeskId = identity.CashDeskId,
					Token = token,
					TimeZone = identity.TimeZone,
					LanguageId = identity.LanguageId,
					PartnerId = identity.PartnerId
				}) as GetCashDeskCurrentBalanceOutput;

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
		private ClientRequestResponseBase Cashout(string token, ApiCashoutInput input, CashierIdentity identity)
		{
			var document = PlatformIntegration.GetBetByDocumentId(new GetBetByDocumentIdInput
			{
				DocumentId = input.BetDocumentId,
				LanguageId = identity.LanguageId,
				PartnerId = identity.PartnerId,
				TimeZone = identity.TimeZone,
				CashDeskId = identity.CashDeskId,
				Token = token,
				IsForPrint = false
			});
			if(Int64.TryParse(document.ExternalId, out long betId))
				input.BetId = betId;

			var response = new PlaceBetOutput { ResponseObject = ProductsIntegration.Cashout(input) };
			try
			{
				var balance = GetBalance(new GetCashDeskCurrentBalanceIntput
				{
					CashDeskId = identity.CashDeskId,
					Token = token,
					TimeZone = identity.TimeZone,
					LanguageId = identity.LanguageId,
					PartnerId = identity.PartnerId
				}) as GetCashDeskCurrentBalanceOutput;

				response.Balance = balance == null ? 0 : balance.Balance;
				response.CurrentLimit = balance == null ? 0 : balance.CurrentLimit;
			}
			catch (Exception e)
			{
				WebApiApplication.LogWriter.Error(e);
			}
			return response;
		}
		private ClientRequestResponseBase PlaceBetByBarcode(GetBetByBarcodeInput input)
		{
			var response = new PlaceBetOutput();
			var bet = PlatformIntegration.GetBetByBarcode(input);
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
				TimeZone = input.TimeZone,
				LanguageId = input.LanguageId,
				PartnerId = input.PartnerId,
				CashierId = input.CashierId,
				CashDeskId = input.CashDeskId
			});
		}
		private ClientRequestResponseBase GetTicketInfo(GetTicketInfoInput input)
		{
			var now = DateTime.UtcNow;
			var bet = PlatformIntegration.GetBetByDocumentId(new GetBetByDocumentIdInput
				{
					DocumentId = Convert.ToInt64(input.TicketId),
					LanguageId = input.LanguageId,
					PartnerId = input.PartnerId,
					TimeZone = input.TimeZone,
					CashDeskId = input.CashDeskId,
					Token = input.Token,
					IsForPrint = true
				});
			if (bet == null || bet.Id == 0)
				return new ClientRequestResponseBase { Description = PlatformIntegration.GetErrorById(new GetErrorInput 
					{ ErrorId = Constants.Errors.TicketNotFound, LanguageId = input.LanguageId }), ResponseCode = Constants.Errors.TicketNotFound };
			if (bet.NumberOfPrints > 3 || (now - bet.CreationTime).TotalMinutes > 30)
				return new ClientRequestResponseBase { Description = PlatformIntegration.GetErrorById(new GetErrorInput
					{ ErrorId = Constants.Errors.CanNotPrintTicket, LanguageId = input.LanguageId }), ResponseCode = Constants.Errors.CanNotPrintTicket };

			var response = ProductsIntegration.GetTicketInfo(input, bet.GameId);
			if (response == null) return null;

			response.BetDate = response.BetDate.GetGMTDateFromUTC(input.TimeZone);
			if (response.BetSelections != null)
			{
				foreach (var bs in response.BetSelections)
				{
					bs.EventDate = bs.EventDate.GetGMTDateFromUTC(input.TimeZone);
				}
			}
			return response;
		}
		private ClientRequestResponseBase GetBetByBarcode(GetBetByBarcodeInput input)
		{
			var clientRequestResponse = PlatformIntegration.GetBetByBarcode(input);
			
			if (clientRequestResponse.Bets == null || !clientRequestResponse.Bets.Any())
				throw new Exception(Constants.Errors.TicketNotFound.ToString());

			var response = ProductsIntegration.GetTicketInfo(new GetTicketInfoInput
			{
				Token = input.Token,
				TimeZone = input.TimeZone,
				LanguageId = input.LanguageId,
				PartnerId = input.PartnerId,
				TicketId = clientRequestResponse.Bets[0].BetDocumentId.ToString()
			}, clientRequestResponse.Bets[0].ProductId);
			
			if (response != null)
			{
				response.BetDate = response.BetDate.GetGMTDateFromUTC(input.TimeZone);
				if (response.BetSelections != null)
				{
					foreach (var bs in response.BetSelections)
					{
						bs.EventDate = bs.EventDate.GetGMTDateFromUTC(input.TimeZone);
					}
				}
				clientRequestResponse.Bets[0].Coefficient = response.Coefficient;
				clientRequestResponse.Bets[0].BetSelections = response.BetSelections;
				clientRequestResponse.Bets[0].NumberOfMatches = response.NumberOfMatches;
				clientRequestResponse.Bets[0].NumberOfBets = response.NumberOfBets;
				clientRequestResponse.Bets[0].AmountPerBet = response.AmountPerBet;
				clientRequestResponse.Bets[0].CommissionFee = response.CommissionFee;
				clientRequestResponse.Bets[0].PossibleWin = response.PossibleWin;
				clientRequestResponse.Bets[0].Barcode = input.Barcode;
				clientRequestResponse.Bets[0].SystemOutCounts = response.SystemOutCounts;
				clientRequestResponse.Bets[0].CashoutAmount = response.CashoutAmount;
				clientRequestResponse.Bets[0].BlockedForCashout = response.BlockedForCashout;
			}
			return clientRequestResponse.Bets[0];
		}
		private ClientRequestResponseBase GetBetInfo(GetTicketInfoInput input)
		{
			var bet = PlatformIntegration.GetBetByDocumentId(new GetBetByDocumentIdInput
				{
					Token = input.Token,
					DocumentId = Convert.ToInt64(input.TicketId),
					TimeZone = input.TimeZone,
					LanguageId = input.LanguageId,
					PartnerId = input.PartnerId,
					CashDeskId = input.CashDeskId,
					IsForPrint = false
				});
			if (bet == null || bet.Id == 0)
				throw new Exception(Constants.Errors.TicketNotFound.ToString());
			var response = ProductsIntegration.GetTicketInfo(input, bet.GameId);
			if (response == null) return null;

			WebApiApplication.LogWriter.Info(JsonConvert.SerializeObject(response));
			response.BetDate = response.BetDate.GetGMTDateFromUTC(input.TimeZone);
			response.Barcode = 0;
			if (response.BetSelections != null)
			{
				foreach (var bs in response.BetSelections)
				{
					bs.EventDate = bs.EventDate.GetGMTDateFromUTC(input.TimeZone);
				}
			}

			return response;
		}
		private ClientRequestResponseBase GetClients(GetClientInput input)
		{
			return PlatformIntegration.GetClients(input);
		}
		private ClientRequestResponseBase GetCashDesks(ApiFilterCashDesk input)
		{
			return PlatformIntegration.GetCashDesks(input);
		}
		private ClientRequestResponseBase GetClient(GetClientInput input)
		{
			return PlatformIntegration.GetClient(input);
		}
		private ClientRequestResponseBase ResetClientPassword(GetClientInput input)
		{
			return PlatformIntegration.ResetClientPassword(input);
		}
		private ClientRequestResponseBase EditClient(ClientModel input)
		{
			return PlatformIntegration.EditClient(input);
		}
		private ClientRequestResponseBase RegisterClient(ClientModel input)
		{
			return PlatformIntegration.RegisterClient(input);
		}
		private ClientRequestResponseBase DepositToInternetClient(DepositToInternetClientInput input)
		{
			var clientRequestResponse = PlatformIntegration.DepositToInternetClient(input);
			return clientRequestResponse;
		}
		private ClientRequestResponseBase GetPaymentRequests(GetPaymentRequestsInput input)
		{
			var clientRequestResponse = PlatformIntegration.GetPaymentRequests(input);
			return clientRequestResponse;
		}
		private ClientRequestResponseBase PayPaymentRequest(PayPaymentRequestInput input)
		{
			var clientRequestResponse = PlatformIntegration.PayPaymentRequest(input);
			return clientRequestResponse;
		}
		private ClientRequestResponseBase PayWin(PayWinInput input)
		{
			var clientRequestResponse = PlatformIntegration.PayWin(input);
			return clientRequestResponse;
		}
		private ClientRequestResponseBase CancelBetSelection(PlatformCancelBetSelectionInput input, CashierIdentity identity)
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
		private ClientRequestResponseBase GetCashDeskInfo(GetCashDeskInfoInput input)
		{
			var clientRequestResponse = PlatformIntegration.GetCashDeskInfo(input);
			return clientRequestResponse;
		}
		private ClientRequestResponseBase CloseShift(CloseShiftInput input)
		{
			var resp = PlatformIntegration.CloseShift(input);
			return resp;
		}
		private ClientRequestResponseBase GetAvailableProducts(string token)
		{
			return new ClientRequestResponseBase();
		}

		private ClientRequestResponseBase GetBalance(GetCashDeskCurrentBalanceIntput input)
		{
			return PlatformIntegration.GetCashDeskCurrentBalance(input);
		}
		private ClientRequestResponseBase CreateWithdrawPaymentRequest(CreatePaymentRequest input)
		{
			return PlatformIntegration.CreateWithdrawPaymentRequest(input);
		}

		#region Reports

		private ClientRequestResponseBase GetBetShopBets(GetBetShopBetsInput input)
		{
			var clientRequestResponse = PlatformIntegration.GetBetShopBets(input);
			return clientRequestResponse;
		}
		private ClientRequestResponseBase GetOperations(GetOperationsInput input)
		{
			var clientRequestResponse = PlatformIntegration.GetBetShopOperations(input);
			return clientRequestResponse;
		}
		private ClientRequestResponseBase GetCashDeskOperations(GetCashDeskOperationsInput input)
		{
			var response = (GetCashDeskOperationsOutput)PlatformIntegration.GetCashDeskOperations(input);
			foreach (var op in response.Operations)
			{
				op.Id = op.TicketNumber ?? 0;
			}
			return response;
		}
		private ClientRequestResponseBase GetShiftReport(GetShiftReportInput input)
		{
			return PlatformIntegration.GetShiftReport(input);
		}
        private ClientRequestResponseBase GetResultsReport(GetResultsReportInput input)
        {
            input.GameId = Convert.ToInt32(Constants.GamesExternalIds.First(x => x.Key == input.GameId).Value);
            return ProductsIntegration.GetResultsReport(input);
        }
        private ClientRequestResponseBase GetUnitResult(GetUnitResultInput input)
        {
            return ProductsIntegration.GetUnitResult(input);
        }
		private ClientRequestResponseBase DepositToTerminal(DepositToInternetClientInput input)
        {
			var clientRequestResponse = PlatformIntegration.DepositToTerminal(input);
			return clientRequestResponse;
		}
		private DepositToInternetClientOutput WithdrawTerminalFunds(PayWinInput input)
		{
			return PlatformIntegration.WithdrawTerminalFunds(input);
		}      

        #endregion
    }
}
