using IqSoft.CP.AutomationTest.Helpers;
using IqSoft.CP.AutomationTest.Models;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IqSoft.CP.AutomationTest.UTests.AdminWebApi
{
    public class BonusSystem
    {
        private static readonly string AdminApiUrl = "http://10.50.17.10:10005/api/Main/{0}";

        [Test]
        public void CreateBonus()
        {
            var newBonus = new
            {
                PartnerId = 1,
                Name = "Cashback1",
                AccountTypeId = 1,
                Status = true,
                StartTime = "2021-11-08T12:08:26.732Z",
                FinishTime = "2022-01-01T12:08:26.732Z",
                Period = 120,
                BonusTypeId = 1,
                MinAmount = 2,
                MaxAmount = 10,
                AutoApproveMaxAmount = 5,
                MaxGranted = 20            
            };
            var baseInput = new
            {
                Controller = "Bonus",
                Method = "CreateBonus",
                Token = "48aa5e17-1f26-4001-9481-b13a5b87e837",
                RequestObject = newBonus
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(AdminApiUrl, "ApiRequest"),
                PostData = JsonConvert.SerializeObject(baseInput)
            };
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(Common1.SendHttpRequest(httpRequestInput));

            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void ApprovedClientCashbackBonus()
        {
            var clientBonusId = 2050;
            var baseInput = new
            {
                Controller = "Client",
                Method = "ApproveClientCashbackBonus",
                Token = "b6602b54-c480-4e46-993b-f1673012d385",
                RequestObject = clientBonusId
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(AdminApiUrl, "ApiRequest"),
                PostData = JsonConvert.SerializeObject(baseInput)
            };
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(Common1.SendHttpRequest(httpRequestInput));

            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

    }
}
