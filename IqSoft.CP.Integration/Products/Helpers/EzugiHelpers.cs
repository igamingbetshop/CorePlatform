using IqSoft.CP.Integration.Products.Models.Ezugi;
using Newtonsoft.Json;
using System;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Integration.Products.Models.IqSoft;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public class EzugiHelpers
    {
        public static string GetMessagesFromProviderServer(string operatorId, string currency)
        {
            string serverUrl = "wss://engine-int.ezugi.com:443/GameServer/gameNotifications";
            var sessionData = new InitializeSessionInput
            {
                MessageType = "InitializeSession",
                OperatorId = Convert.ToInt32(operatorId),
                VipLevel = 0,
                SessionCurrency = currency
            };
            var requestString = JsonConvert.SerializeObject(sessionData);
            var msg = string.Format("{0}{1}", requestString, Environment.NewLine);

            EzugiHelpers eH = new EzugiHelpers();
            var serverResponse = eH.ReconnectToServer(serverUrl, msg);

            return serverResponse;
        }

        private string data = string.Empty;
     //   private bool completed = false;

        private string ReconnectToServer(string hostName, string message)
        {
            //using (var ws = new WebSocket(hostName))
            //{
            //    ws.OnMessage += (sender, e) => ChangeReceivedInfo(e.Data);
            //    ws.OnError += (sender, e) => { throw new ArgumentNullException(string.Format("Cannot connect to Ezugi's server: {0}", e.Message)); };

            //    ws.Connect();
            //    ws.Send(message);
            //    while (!completed)
            //    {
            //        Thread.Sleep(500);
            //    }
            //}
            return data;
        }

        private void ChangeReceivedInfo(string newData)
        {
            data += newData;
           // completed = true;
        }

        public static CurrentLiveGamesReport GetReport(string roundId, DateTime betTime)
        {
            var providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Ezugi).Id;
            var apiId = CacheManager.GetGameProviderValueByKey(0, providerId, Constants.PartnerKeys.EzugiApiId);
            var apiName = CacheManager.GetGameProviderValueByKey(0, providerId, Constants.PartnerKeys.EzugiApiName);
            var access = CacheManager.GetGameProviderValueByKey(0, providerId, Constants.PartnerKeys.EzugiAccess);
            var url = CacheManager.GetGameProviderValueByKey(0, providerId, Constants.PartnerKeys.EzugiApiUrl);
            var reportInput = new ReportInput
            {
                DataSet = "game_rounds",
                APIID = apiId,
                APIUser = apiName,
                RoundID = roundId,
                StartTime = betTime.AddDays(-1).ToString("yyyy-MM-dd hh:mm:ss"),
                EndTime = betTime.AddDays(1).ToString("yyyy-MM-dd hh:mm:ss")
            };
            var token = access + CommonFunctions.GetUriDataFromObject(reportInput);
            reportInput.RequestToken = CommonFunctions.ComputeSha256(token);

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                PostData = CommonFunctions.GetUriEndocingFromObject(reportInput),
                Url = url
            };
            var reportOutput = JsonConvert.DeserializeObject<ReportOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            var response = new CurrentLiveGamesReport
            {
                RoundResult = new List<Models.IqSoft.Data>()
            };
            foreach (var data in reportOutput.Datas)
            {
                response.RoundResult.Add(new Models.IqSoft.Data
                {
                    RoundId = data.RoundId,
                    TableID = data.TableId,
                    RoundDateTime = data.RoundDateTime.ToString(),
                    DealerName = data.DealerName,
                    DealerId = data.DealerId,
                    Results = JsonConvert.DeserializeObject<Round>(data.RoundString)
                });
            }
            return response;
        }
    }
}