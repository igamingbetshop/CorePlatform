using IqSoft.CP.DistributionWebApi.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[RoutePrefix("walletone")]
	public class WalletOneController : ApiController
    {
        [HttpPost]
        public string CallWalletOneDepositApi(RequestInput request)
        {
            try
            {
                var input = new HttpRequestInput
                {
                    ContentType = HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpRequestMethods.Post,
                    Url = "https://wl.walletone.com/checkout/checkout/Index",
                    PostData = request.Content
                };
				string contentType;
                var response = Common.SendHttpRequest(input, out contentType);

                int startIndex = response.IndexOf("/checkout/checkout/ReturnToPaymentTree?i=", 0);
                int endIndex = response.IndexOf("\"", startIndex);
                response = response.Substring(startIndex, endIndex - startIndex);
                response = "https://wl.walletone.com" + response;
                return response;
            }
            catch (Exception exc)
            {
                return "Catch message:" + exc.Message + "\n request.Content:" + request.Content;
            }
        }

        [HttpPost]
        public string CallWalletOneWithdrawApi(WalletOneWithdrawRequest request)
        {
            try
            {
                Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
                requestHeaders.Add("Authorization", "Bearer " + request.Key);

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = "application/vnd.wallet.openapi.v1+json",// Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = request.RequestMethod,
                    // Url = request.Url,
                    PostData = request.Content,
                    Accept = "application/vnd.wallet.openapi.v1+json",
                    RequestHeaders = requestHeaders
                };
                httpRequestInput.Url = request.Url;
				string contentType;
				return Common.SendHttpRequest(httpRequestInput, out contentType);
            }
            catch (Exception exc)
            {
                return "Catch message:" + exc.Message + "\n request.Content:" + request.Content;
            }
        }
    }
}
