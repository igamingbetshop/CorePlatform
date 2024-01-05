using IqSoft.CP.AutomationTest.Helpers;
using IqSoft.CP.AutomationTest.Models;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;

namespace IqSoft.CP.AutomationTest.UTests.AdminWebApi
{
    public class ClientSegmentation
    {
        private static readonly string AdminApiUrl = "https://adminwebapi.iqsoftllc.com/api/Main/{0}";

        [Test]
        public void CreateSegment()
        {
            var newSegment = new
            {
                Id= 0,
                Name = "S2",
                PartnerId = 1,
                State = 1,
                Mode = 1,                
                TotalDepositAmount = new Condition
                {
                    ConditionItems = new List<ConditionItem>
                    {
                       new ConditionItem { OperationTypeId = 2, StringValue = "100"  }
                    }
                },
            };
            var baseInput = new
            {
                Controller = "Content",
                Method = "SaveSegment",
                Token = "7c092e74-d70d-4d64-81e7-45c4bd71b041",
                RequestObject = newSegment
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
