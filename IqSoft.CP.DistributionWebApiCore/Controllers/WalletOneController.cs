using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[Route("walletone")]
    [ApiController]
    public class WalletOneController : ControllerBase
    {
        [HttpPost]
        [Route("CallWalletOneDepositApi"), HttpPost]
        public string CallWalletOneDepositApi(RequestInput request)
        {
            try
            {
                var input = new HttpRequestInput
                {
                    ContentType = HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = "https://wl.walletone.com/checkout/checkout/Index",
                    PostData = request.Content
                };
                var response = Common.SendHttpRequest(input, out _);

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

        [Route("CallWalletOneWithdrawApi"), HttpPost]
        public string CallWalletOneWithdrawApi(WalletOneWithdrawRequest request)
        {
            try
            {
                Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
                requestHeaders.Add("Authorization", "Bearer " + request.Key);

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = "application/vnd.wallet.openapi.v1+json",// Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod =new HttpMethod( request.RequestMethod),
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
