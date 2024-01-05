using IqSoft.CP.AutomationTest.Helpers;
using IqSoft.CP.AutomationTest.Models;
using IqSoft.CP.Common;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IqSoft.CP.AutomationTest.UTests
{

    public class ClientTest
    {
        private static readonly string ApiUrl = "https://websitewebapi.perubet.live/3/api/Main/{0}";
        private static readonly string MainCurrency = "EUR";

        [SetUp]
        public void Setup()
        {
            //  WebDriver.Navigate().GoToUrl("https://craftbet.com");
        }

        [Test]
        public void LoginClient()
        {



            var s = "{\"ExternalPlatformId\":1,\"ClientIdentifier\":\"SVan4HrIbyTc_3um2vNTv41tCUCz43Qj1_a1PUUNI6A\",\"PartnerId\":52,\"LanguageId\":\"en\",\"DeviceType\":1}";
            var i = "{\"ExternalPlatformId\":1,\"ClientIdentifier\":\"DcB54XeewMPWp4oHumDdPEzp4gTT-NrfhkaNrQv_zxY\",\"PartnerId\":52,\"LanguageId\":\"en\",\"DeviceType\":2}";
            var encrypted = RSAEncryption.RSAEncryptByPEM(i);





            var response = LoginClient("1qG7CYIpmJ92ffrOUBhdd2ONnOhO6yV0CLP816uFKfA", "Suzanna1");

            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }
        [Test]
        public void Test4()
        {
            try
            {
                var connection = new HubConnectionBuilder()
            .WithUrl("https://adminwebapi.poppabet.com/api/signalr/basehub")
            //.WithConsoleLogger()
            .Build();
                connection.StartAsync().Wait();
                var input = new
                {
                    Token = "4bd91186-31a7-4619-967d-723c337d40f1",
                    TimeZone = 4,
                    LanguageId = "en",
                    TicketId = 93
                    //ClientIds = new ApiFiltersOperation
                    //{
                    //    IsAnd = true,
                    //    ApiOperationTypeList= new List<ApiFiltersOperationType> {
                    //        new ApiFiltersOperationType{IntValue = 15, DecimalValue = 15,OperationTypeId=1}}
                    //}
                };
                var res = connection.InvokeAsync<ApiResponseBase>("CloseTicket", input).Result;
            }
            catch { }
        

        }

        public static LoginOutput LoginClient(string clientIdentifier, string password)
        {
            var loginInput = new
            {
                ClientIdentifier = clientIdentifier,
                Password = password,
                ExternalPlatformId = 1
            };
            var encrypted = RSAEncryption.RSAEncryptByPEM(JsonConvert.SerializeObject(loginInput));
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "LoginClient"),
                PostData = JsonConvert.SerializeObject(new { Data = encrypted })
            };
            return JsonConvert.DeserializeObject<LoginOutput>(Common1.SendHttpRequest(httpRequestInput));
        }
        [Test]
        public void QuickEmailRegistration()
        {
            var quickEmailRegistrationInput = new
            {
                Email = "s.sargsyan@iqsoft.am",
                Password = "Suzanna1",
                CurrencyId = "USD",
                TermsConditionsAccepted = true
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "QuickEmailRegistration"),
                PostData = JsonConvert.SerializeObject(quickEmailRegistrationInput)
            };
            var response = JsonConvert.DeserializeObject<LoginOutput>(Common1.SendHttpRequest(httpRequestInput));

            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void QuickMobileRegistration()
        {
            var quickEmailRegistrationInput = new
            {
                MobileNumber = "+37491010101",
                Password = "TestP@ssw0rd",
                CurrencyId = MainCurrency,
                TermsConditionsAccepted = true
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "QuickSmsRegistration"),
                PostData = JsonConvert.SerializeObject(quickEmailRegistrationInput)
            };
            var response = JsonConvert.DeserializeObject<LoginOutput>(Common1.SendHttpRequest(httpRequestInput));

            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void FullRegistration()
        {
            var fullRegistrationInput = new
            {
                UserName = "LuckyMan",
                FirstName = "Lucky",
                LastName = "Man",
                MobileNumber = "+37491010101",
                Password = "TestP@ssw0rd",
                CurrencyId = MainCurrency,
                TermsConditionsAccepted = true,
                Gender = 2, //Male = 1, Female = 2
                BirthMonth = 5,
                BirthYear = 2000,
                BirthDay = 23
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "RegisterClient"),
                PostData = JsonConvert.SerializeObject(fullRegistrationInput)
            };
            var response = JsonConvert.DeserializeObject<LoginOutput>(Common1.SendHttpRequest(httpRequestInput));

            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }

        [Test]
        public void GetClientByToken()
        {
            var loginResponse = ClientTest.LoginClient("s.sargsyan@iqsoft.am", "Suzanna1");
            if (loginResponse.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(loginResponse.Description);
            var getClientByTokenInput = new
            {
                LanguageId = "en",
                Token = loginResponse.Token,
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format(ApiUrl, "GetClientByToken"),
                PostData = JsonConvert.SerializeObject(getClientByTokenInput)
            };
            var response = JsonConvert.DeserializeObject<LoginOutput>(Common1.SendHttpRequest(httpRequestInput));

            if (response.ResponseCode == 0)
                Assert.Pass();
            else
                Assert.Fail(response.Description);
        }
    }
}