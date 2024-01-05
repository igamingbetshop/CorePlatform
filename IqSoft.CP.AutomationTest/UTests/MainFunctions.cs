using IqSoft.CP.AutomationTest.Helpers;
using IqSoft.CP.AutomationTest.Models;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using NUnit.Framework;
namespace IqSoft.CP.AutomationTest.UTests
{
   public class MainFunctions
    {
        private static readonly string ApiUrl = "https://websitewebapi.craftbet.com/1/api/Main/{0}";

        [SetUp]
        public void Setup()
        {
        }
        [Test]
        public void GeolocationData()
        {
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "GeolocationData"),
                PostData = "{}"
            };
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(Common1.SendHttpRequest(httpRequestInput));
            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void GetProductUrl()
        {
            var request = new
            {
                IsForMobile = false,
                LanguageId = "en",
                PartnerId = 1,
                ProductId = 6,
                Position = "prematch",
                IsForDemo = true
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "GetProductUrl"),
                PostData = JsonConvert.SerializeObject(request)
            };
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(Common1.SendHttpRequest(httpRequestInput));
            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void GetProductUrlWithToken()
        {
            var loginOutput = ClientTest.LoginClient("s.sargsyan@iqsoft.am", "Suzanna1");
            if (loginOutput.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(loginOutput.Description);
            var request = new
            {
                ClientId = loginOutput.Id,
                IsForMobile = false,
                LanguageId = "en",
                PartnerId = 1,
                ProductId = 6,
                Position = "prematch",
                IsForDemo = false,
                Token = loginOutput.Token
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "GetProductUrl"),
                PostData = JsonConvert.SerializeObject(request)
            };
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(Common1.SendHttpRequest(httpRequestInput));
            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void GetRegions()
        {
            var request = new
            {
                LanguageId = "en",
                PartnerId = 1,
                TypeId = 5 // coutries
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "GetRegions"),
                PostData = JsonConvert.SerializeObject(request)
            };
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(Common1.SendHttpRequest(httpRequestInput));
            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

    }
}
