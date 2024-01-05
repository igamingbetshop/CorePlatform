using IqSoft.CP.AutomationTest.Helpers;
using IqSoft.CP.AutomationTest.Models;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace IqSoft.CP.AutomationTest.UTests
{
    public class ApiRequestTests
    {
        private static readonly string ApiUrl = "https://websitewebapi.craftbet.com/1/api/Main/ApiRequest";

        public static ApiResponseBase ApiRequest(object input)
        {
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = ApiUrl,
                PostData = JsonConvert.SerializeObject(input)
            };
            var response = JsonConvert.DeserializeObject<ApiResponseBase>(Common1.SendHttpRequest(httpRequestInput));
            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
            return response;
        }

        [Test]
        public void GetClientBalance()
        {
            TimeSpan ts = TimeSpan.FromTicks(6000000);
            double minutes = ts.TotalMinutes;


            var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = System.Net.Http.HttpMethod.Delete,
                    Url = "https://test-api.paymentiq.io/paymentiq/api/user/transaction/100618998/282082/1019192867?sessionId=3b20f5965c1c4a51a29a5d022e11bf33",
                    PostData = "{}"
                };
                var response1 = Common1.SendHttpRequest(httpRequestInput);

            



            var loginResponse = ClientTest.LoginClient("s.sargsyan@iqsoft.am", "Suzanna1");
            if (loginResponse.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(loginResponse.Description);

            var input = new
            {
                ClientId = loginResponse.Id,
                Token = loginResponse.Token
            };
            var response = ApiRequest(input);
            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void TestStoredProcedure()
        {
            try
            {
                using (var db = new IqSoftCorePlatformEntities())
                {
                    // using (var scope = CommonFunctions.CreateTransactionScope())
                    {
                        //var d = db.sp_GetDbDate();
                        //var r = db.sp_GetAccountLockById(118);
                        //var res = db.sp_GetPaymentRequestLock(161417);
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
    }
}