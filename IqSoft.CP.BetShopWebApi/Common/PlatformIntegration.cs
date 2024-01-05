using IqSoft.CP.BetShopGatewayWebApi.Common;
using IqSoft.CP.BetShopGatewayWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;

namespace IqSoft.CP.BetShopWebApi.Common
{
	public static class PlatformIntegration
	{
		private static readonly string PlatformUrl = ConfigurationManager.AppSettings["PlatformBetShopClientGatewayUrl"];
		private const string PlatformRequestUrlFormat = "{0}/{1}?TimeZone={2}&LanguageId={3}&PartnerId={4}";

		private static T SendRequestToPlatform<T>(PlatformRequestBase input, string method) where T : ClientRequestResponseBase
		{
			var url = string.Format(PlatformRequestUrlFormat, PlatformUrl, method, input.TimeZone, input.LanguageId, input.PartnerId);
			var requestInput = new HttpRequestInput
			{
				Url = url,
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				PostData = JsonConvert.SerializeObject(input)
			};
			var responseStr = CommonFunctions.SendHttpRequest(requestInput, out _);
			var response = JsonConvert.DeserializeObject<T>(responseStr);
			return response;
		}

		public static AuthorizationOutput CardReaderAuthorization(AuthorizationInput input)
		{
			try
			{
				return SendRequestToPlatform<AuthorizationOutput>(input, ApiMethods.CardReaderAuthorization);
			}
			catch (Exception ex)
			{
				return new AuthorizationOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static AuthorizationOutput Authorization(AuthorizationInput input)
		{
			try
			{
				return SendRequestToPlatform<AuthorizationOutput>(input, ApiMethods.Authorization);
			}
			catch (Exception ex)
			{
				return new AuthorizationOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static AuthorizationOutput Login(AuthorizationInput input)
		{
			try
			{
				return SendRequestToPlatform<AuthorizationOutput>(input, ApiMethods.Login);
			}
			catch (Exception ex)
			{
				return new AuthorizationOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static AuthorizationOutput GetCashierSessionByToken(GetCashierSessionInput input)
		{
			try
			{
				return SendRequestToPlatform<AuthorizationOutput>(input, ApiMethods.GetCashierSessionByToken);
			}
			catch (Exception ex)
			{
				return new AuthorizationOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static AuthorizationOutput GetCashierSessionByProductId(GetCashierSessionInput input)
		{
			try
			{
				return SendRequestToPlatform<AuthorizationOutput>(input, ApiMethods.GetCashierSessionByProductId);
			}
			catch (Exception ex)
			{
				return new AuthorizationOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase GetClients(GetClientInput input)
		{
			try
			{
				return SendRequestToPlatform<ApiGetClientsOutput>(input, ApiMethods.GetClients);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase GetCashDesks(ApiFilterCashDesk input)
		{
			try
			{
				return SendRequestToPlatform<CashDesksOutput>(input, ApiMethods.GetCashDesks);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static GetClientOutput GetClient(GetClientInput input)
		{
			try
			{
				return SendRequestToPlatform<GetClientOutput>(input, ApiMethods.GetClient);
			}
			catch (Exception ex)
			{
				return new GetClientOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ApiClientLogin ResetClientPassword(GetClientInput input)
		{
			try
			{
				return SendRequestToPlatform<ApiClientLogin>(input, ApiMethods.ResetClientPassword);
			}
			catch (Exception ex)
			{
				return new ApiClientLogin { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase EditClient(ClientModel input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.EditClient);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ApiClientOutput RegisterClient(ClientModel input)
		{
			try
			{
				return SendRequestToPlatform<ApiClientOutput>(input, ApiMethods.RegisterClient);
			}
			catch (Exception ex)
			{
				return new ApiClientOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static GetProductSessionOutput GetProductSession(GetProductSessionInput input)
		{
			try
			{
				return SendRequestToPlatform<GetProductSessionOutput>(input, ApiMethods.GetProductSession);
			}
			catch (Exception ex)
			{
				return new GetProductSessionOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase DepositToInternetClient(DepositToInternetClientInput input)
		{
			try
			{
				return SendRequestToPlatform<DepositToInternetClientOutput>(input, ApiMethods.DepositToInternetClient);
			}
			catch (Exception ex)
			{
				return new DepositToInternetClientOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase RollbackDepositToInternetClient(RollbackDepositToInternetClient input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.RollbackDepositToInternetClient);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase GetPaymentRequests(GetPaymentRequestsInput input)
		{
			try
			{
				return SendRequestToPlatform<GetPaymentRequestsOutput>(input, ApiMethods.GetPaymentRequests);
			}
			catch (Exception ex)
			{
				return new GetPaymentRequestsOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase PayPaymentRequest(PayPaymentRequestInput input)
		{
			try
			{
				return SendRequestToPlatform<PayPaymentRequestOutput>(input, ApiMethods.PayPaymentRequest);
			}
			catch (Exception ex)
			{
				return new FinOperationResponse { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase PayWin(PayWinInput input)
		{
			try
			{
				return SendRequestToPlatform<FinOperationResponse>(input, ApiMethods.PayWin);
			}
			catch (Exception ex)
			{
				return new FinOperationResponse { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static GetBetShopBetsOutput GetBetByBarcode(GetBetByBarcodeInput input)
		{
			try
			{
				return SendRequestToPlatform<GetBetShopBetsOutput>(input, ApiMethods.GetBetByBarcode);
			}
			catch (Exception ex)
			{
				return new GetBetShopBetsOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase GetCashDeskInfo(GetCashDeskInfoInput input)
		{
			try
			{
				return SendRequestToPlatform<GetCashDeskInfoOutput>(input, ApiMethods.GetCashDeskInfo);
			}
			catch (Exception ex)
			{
				return new GetCashDeskInfoOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase GetShiftReport(GetShiftReportInput input)
		{
			try
			{
				return SendRequestToPlatform<GetShiftReportOutput>(input, ApiMethods.GetShiftReport);
			}
			catch (Exception ex)
			{
				return new GetShiftReportOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
        public static ApiRegionOutput GetRegions(ApiRegionInput input)
        {
            try
            {
				return SendRequestToPlatform<ApiRegionOutput>(input, ApiMethods.GetRegions);
            }
            catch (Exception ex)
            {
                return new ApiRegionOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
            }
        }
        public static ClientRequestResponseBase CloseShift(CloseShiftInput input)
		{
			try
			{
				var resp = SendRequestToPlatform<CloseShiftOutput>(input, ApiMethods.CloseShift);
				return resp;
			}
			catch (Exception ex)
			{
				return new CloseShiftOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase LogoutUser(CloseSessionInput input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.CloseSession);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase ChangeCashierPassword(ChangePasswordInput input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.ChangeCashierPassword);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase AssignPin(AssignPinInput input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.AssignPin);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase GetBetShopBets(GetBetShopBetsInput input)
		{
			try
			{
				var resp = SendRequestToPlatform<GetBetShopBetsOutput>(input, ApiMethods.GetBetShopBets);
				foreach (var b in resp.Bets)
				{
					b.Barcode = null;
				}
				return resp;
			}
			catch (Exception ex)
			{
				return new GetBetShopBetsOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase CreateDebitCorrectionOnCashDesk(CashDeskCorrectionInput input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.CreateDebitCorrectionOnCashDesk);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase CreateCreditCorrectionOnCashDesk(CashDeskCorrectionInput input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.CreateCreditCorrectionOnCashDesk);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase GetCashDeskOperations(GetCashDeskOperationsInput input)
		{
			try
			{
				return SendRequestToPlatform<GetCashDeskOperationsOutput>(input, ApiMethods.GetCashDeskOperations);
			}
			catch (Exception ex)
			{
				return new GetCashDeskOperationsOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static GetCashDesksBalanceOutput GetCashDesksBalance(GetCashDeskBalanceIntput input)
		{
			try
			{
				return SendRequestToPlatform<GetCashDesksBalanceOutput>(input, ApiMethods.GetCashDesksBalancesByDate);
			}
			catch (Exception ex)
			{
				return new GetCashDesksBalanceOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static GetCashDeskCurrentBalanceOutput GetCashDeskCurrentBalance(GetCashDeskCurrentBalanceIntput input)
		{
			try
			{
				return SendRequestToPlatform<GetCashDeskCurrentBalanceOutput>(input, ApiMethods.GetCashDeskCurrentBalance);
			}
			catch (Exception ex)
			{
				return new GetCashDeskCurrentBalanceOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static GetBetShopOperationsOutput GetBetShopOperations(GetOperationsInput input)
		{
			try
			{
				return SendRequestToPlatform<GetBetShopOperationsOutput>(input, ApiMethods.GetBetShopOperations);
			}
			catch (Exception ex)
			{
				return new GetBetShopOperationsOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static GetBetByDocumentIdOutput GetBetByDocumentId(GetBetByDocumentIdInput input)
		{
			try
			{
				return SendRequestToPlatform<GetBetByDocumentIdOutput>(input, ApiMethods.GetBetByDocumentId);
			}
			catch (Exception ex)
			{
				return new GetBetByDocumentIdOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static ClientRequestResponseBase DepositToTerminal(DepositToInternetClientInput input)
		{
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.DepositToTerminal);
			}
			catch (Exception ex)
			{
				return new ClientRequestResponseBase { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static DepositToInternetClientOutput WithdrawTerminalFunds(PayWinInput input)
		{
			try
			{
				return SendRequestToPlatform<DepositToInternetClientOutput>(input, ApiMethods.WithdrawTerminalFunds);
			}
			catch (Exception ex)
			{
				return new DepositToInternetClientOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static PayPaymentRequestOutput CreateWithdrawPaymentRequest(CreatePaymentRequest input)
		{
			try
			{
				return SendRequestToPlatform<PayPaymentRequestOutput>(input, ApiMethods.CreateWithdrawPaymentRequest);
			}
			catch (Exception ex)
			{
				return new PayPaymentRequestOutput { ResponseCode = Constants.Errors.GeneralException, Description = ex.Message };
			}
		}
		public static string GetErrorById(GetErrorInput input)
        {
			try
			{
				return SendRequestToPlatform<ClientRequestResponseBase>(input, ApiMethods.GetErrorById).Description.ToString();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}