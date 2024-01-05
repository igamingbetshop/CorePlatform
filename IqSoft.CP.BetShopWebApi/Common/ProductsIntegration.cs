﻿using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApi.Models.Reports;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace IqSoft.CP.BetShopWebApi.Common
{
    public static class ProductsIntegration
	{
		public static readonly string VirtualGamesBetShopWebApiUrl = ConfigurationManager.AppSettings["VirtualGamesBetShopWebApiUrl"];
		public static readonly string SportsbookBetShopWebApiUrl = ConfigurationManager.AppSettings["SportsbookBetShopWebApiUrl"];
		public static readonly string BetShopCredentials = ConfigurationManager.AppSettings["BetShopCredentials"];
		public static readonly string IqSoftBrandId = ConfigurationManager.AppSettings["IqSoftBrandId"];

		public static List<BetOutput> DoBet(DoBetInput betInput, int productId)
		{
			var responseStr = SendRequest(productId, ApiMethods.DoBet, JsonConvert.SerializeObject(betInput));
			var betOutput = JsonConvert.DeserializeObject<PlaceBetOutput>(responseStr);
			return betOutput.Bets;
		}
		public static ClientRequestResponseBase Cashout(ApiCashoutInput input)
		{
			var responseStr = SendRequest(Constants.Games.Sportsbook, ApiMethods.Cashout, JsonConvert.SerializeObject(input));
			var response = JsonConvert.DeserializeObject<ClientRequestResponseBase>(responseStr);
			return response;
		}
		public static BetOutput GetBookedBet(GetTicketInfoInput requestInput)
		{
			var responseStr = SendRequest(Constants.Games.Sportsbook, ApiMethods.GetBookedBet,
				JsonConvert.SerializeObject(new { requestInput.Token, requestInput.Code, requestInput.LanguageId, requestInput.TimeZone, requestInput.PartnerId }));
			if (string.IsNullOrEmpty(responseStr)) 
				return null;
			WebApiApplication.LogWriter.Info(responseStr);

			var betOutput = JsonConvert.DeserializeObject<BetShopRequestOutput>(responseStr);
			if (betOutput.ResponseCode != Constants.SuccessResponseCode)
				return new BetOutput { ResponseCode = betOutput.ResponseCode, Description = betOutput.Description };

			var response = JsonConvert.DeserializeObject<BetOutput>(betOutput.ResponseObject.ToString());
			foreach (var selection in response.BetSelections)
			{
				selection.ProductId = Constants.GamesExternalIds.First(x => x.Value == selection.ProductId.ToString()).Key;
			}
			return response;
		}
		
		public static BetOutput GetTicketInfo(GetTicketInfoInput requestInput, int productId)
		{
			var responseStr = SendRequest(productId, ApiMethods.GetTicketInfo,
				JsonConvert.SerializeObject(
					new { requestInput.Token, requestInput.TicketId, requestInput.LanguageId, requestInput.TimeZone, 
						PartnerId = (productId == (int)Constants.Games.IqSoftSportsbook ? IqSoftBrandId : requestInput.PartnerId.ToString()) }));
			if (string.IsNullOrEmpty(responseStr)) return null;

			var betOutput = JsonConvert.DeserializeObject<BetShopRequestOutput>(responseStr);
			if (betOutput.ResponseCode != Constants.SuccessResponseCode)
				return new BetOutput { ResponseCode = betOutput.ResponseCode, Description = betOutput.Description };

			return JsonConvert.DeserializeObject<BetOutput>(betOutput.ResponseObject.ToString());
		}

		public static ClientRequestResponseBase CancelBetSelection(CancelBetSelectionInput cancelInput, int productId)
		{
			var responseStr = SendRequest(productId, ApiMethods.CancelBetSelection, JsonConvert.SerializeObject(cancelInput));
			if (!string.IsNullOrEmpty(responseStr))
			{
				var cancelOutput = JsonConvert.DeserializeObject<BetShopRequestOutput>(responseStr);
				if (cancelOutput.ResponseCode != Constants.SuccessResponseCode)
					return new CancelBetSelectionOutput
					{
						ResponseCode = cancelOutput.ResponseCode,
						Description = cancelOutput.Description
					};
			}
			return new CancelBetSelectionOutput();
		}

        public static GetResultsReportOutput GetResultsReport(GetResultsReportInput input)
        {
            var responseStr = SendRequest(Constants.Games.Keno, ApiMethods.GetUnitResults, JsonConvert.SerializeObject(input));
            if (!string.IsNullOrEmpty(responseStr))
            {
                var output = JsonConvert.DeserializeObject<ClientRequestResponseBase>(responseStr);
                return JsonConvert.DeserializeObject<GetResultsReportOutput>(JsonConvert.SerializeObject(output.ResponseObject));
            }
            return new GetResultsReportOutput { ResponseCode = Constants.Errors.GeneralException };
        }

        public static GetUnitResultInfoOutput GetUnitResult(GetUnitResultInput input)
        {
            var responseStr = SendRequest(Constants.Games.Keno, ApiMethods.GetUnitInfo, JsonConvert.SerializeObject(input));
            if (!string.IsNullOrEmpty(responseStr))
            {
                var output = JsonConvert.DeserializeObject<ClientRequestResponseBase>(responseStr);
                return JsonConvert.DeserializeObject<GetUnitResultInfoOutput>(JsonConvert.SerializeObject(output.ResponseObject));
            }
            return new GetUnitResultInfoOutput { ResponseCode = Constants.Errors.GeneralException };
        }

		private static string SendRequest(int productId, string requestMethod, string requestObject)
		{
			string url;
			switch (productId)
			{
				case Constants.Games.Keno:
				case Constants.Games.BetOnPoker:
				case Constants.Games.BetOnRacing:
				case Constants.Games.Bingo37:
				case Constants.Games.Colors:
				case Constants.Games.Bingo48:
					url = VirtualGamesBetShopWebApiUrl + "/" + requestMethod;
					break;
				case Constants.Games.Sportsbook:
				case Constants.Games.IqSoftSportsbook:
					url = SportsbookBetShopWebApiUrl + "/" + requestMethod;
					break;
				default:
					return string.Empty;
			}

			var input = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = url,
				PostData = requestObject
			};
			
			return CommonFunctions.SendHttpRequest(input, out _);
		}
	}
}