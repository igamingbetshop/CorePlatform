using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApi.Models.Reports;
using IqSoft.CP.BetShopWebApiCore;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IqSoft.CP.BetShopWebApi.Common
{
    public static class ProductsIntegration
    {
        public static List<BetOutput> DoBet(DoBetInput betInput, int productId)
        {
            var responseStr = SendRequest(productId, ApiMethods.DoBet, JsonConvert.SerializeObject(betInput));
            var betOutput = JsonConvert.DeserializeObject<PlaceBetOutput>(responseStr);
            return betOutput.Bets;
        }
        public static ApiResponseBase Cashout(ApiCashoutInput input)
        {
            var responseStr = SendRequest(Constants.Games.Sportsbook, ApiMethods.Cashout, JsonConvert.SerializeObject(input));
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(responseStr);
            return response;
        }
        public static BetOutput GetBookedBet(GetTicketInfoInput requestInput)
        {
            var responseStr = SendRequest(Constants.Games.Sportsbook, ApiMethods.GetBookedBet,
                JsonConvert.SerializeObject(new { requestInput.Token, requestInput.Code, requestInput.LanguageId, requestInput.TimeZone, requestInput.PartnerId }));
            if (string.IsNullOrEmpty(responseStr))
                return null;
            Log.Information(responseStr);

            var betOutput = JsonConvert.DeserializeObject<ApiResponseBase>(responseStr);
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
                    new
                    {
                        requestInput.Token,
                        requestInput.TicketId,
                        requestInput.LanguageId,
                        requestInput.TimeZone,
                        PartnerId = (productId == (int)Constants.Games.IqSoftSportsbook ? Program.AppSetting.IqSoftBrandId : requestInput.PartnerId.ToString())
                    }));
            if (string.IsNullOrEmpty(responseStr)) return null;

            var betOutput = JsonConvert.DeserializeObject<ApiResponseBase>(responseStr);
            if (betOutput.ResponseCode != Constants.SuccessResponseCode)
                return new BetOutput { ResponseCode = betOutput.ResponseCode, Description = betOutput.Description };

            return JsonConvert.DeserializeObject<BetOutput>(betOutput.ResponseObject.ToString());
        }

        public static ApiResponseBase CancelBetSelection(CancelBetSelectionInput cancelInput, int productId)
        {
            var responseStr = SendRequest(productId, ApiMethods.CancelBetSelection, JsonConvert.SerializeObject(cancelInput));
            if (!string.IsNullOrEmpty(responseStr))
                return JsonConvert.DeserializeObject<ApiResponseBase>(responseStr);
            return new ApiResponseBase
            {
                ResponseCode = Constants.Errors.GeneralException,
                Description = responseStr
            };
        }

        public static GetResultsReportOutput GetResultsReport(GetResultsReportInput input)
        {
            var responseStr = SendRequest(Constants.Games.Keno, ApiMethods.GetUnitResults, JsonConvert.SerializeObject(input));
            if (!string.IsNullOrEmpty(responseStr))
            {
                var output = JsonConvert.DeserializeObject<ApiResponseBase>(responseStr);
                return JsonConvert.DeserializeObject<GetResultsReportOutput>(JsonConvert.SerializeObject(output.ResponseObject));
            }
            return new GetResultsReportOutput { ResponseCode = Constants.Errors.GeneralException };
        }
        public static GetUnitResultInfoOutput GetUnitResult(GetUnitResultInput input)
        {
            var responseStr = SendRequest(Constants.Games.Keno, ApiMethods.GetUnitInfo, JsonConvert.SerializeObject(input));
            if (!string.IsNullOrEmpty(responseStr))
            {
                var output = JsonConvert.DeserializeObject<ApiResponseBase>(responseStr);
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
                case Constants.Games.KenoPlus:
                case Constants.Games.NewKeno:
                case Constants.Games.BetOnPoker:
                case Constants.Games.BetOnRacing:
                case Constants.Games.Bingo37:
                case Constants.Games.Colors:
                case Constants.Games.Bingo48:
                case Constants.Games.SpinAndWin:
                    url = Program.AppSetting.VirtualGamesBetShopWebApiUrl + "/" + requestMethod;
                    break;
                case Constants.Games.Sportsbook:
                case Constants.Games.IqSoftSportsbook:
                    url = Program.AppSetting.SportsbookBetShopWebApiUrl + "/" + requestMethod;
                    break;
                default:
                    return string.Empty;
            }

            var input = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                Url = url,
                PostData = requestObject
            };

            return CommonFunctions.SendHttpRequest(input, out _);
        }
    }
}