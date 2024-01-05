using System;
using System.Configuration;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Interfaces;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Integration.ControlSystem;
using log4net;
using Newtonsoft.Json;

namespace IqSoft.CP.BLL.Services
{
    public  class IntegrationBll : PermissionBll, IIntegrationBll
    {
        #region Constructors

        public IntegrationBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public IntegrationBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        #region ControlSystem

        public const int ControlSystemSuccesResponseCode = 0;
        public const int ControlSystemGeneralErrorCode = 0;

        public static class ControlSystemMethods
        {
            public const string DoBet = "DoBet";
            public const string DoWin = "DoWin";
            public const string DoPay = "DoPay";
        }
		
        // Send BetShop Wins to ControlSystem
        public void SendWinsToControlSystem(BetShopFinOperationsOutput transactions)
        {
            try
            {
                foreach (var operation in transactions.Documents)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var finOperationInput = new FinOperationInput
                        {
                            Id = operation.Id.ToString(),
                            ParentId = operation.ParentId.ToString(),
                            WinAmount = operation.Amount,
                            ResultInfo = operation.Info
                        };
                        var response = SendOperationToControlSystem(finOperationInput, ControlSystemMethods.DoWin);
                        if (response.ResponseCode == ControlSystemSuccesResponseCode)
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        // Send BetShop Pays to ControlSystem
        public void SendPayToControlSystem(long id, long parentId)
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    var finOperationInput = new FinOperationInput
                    {
                        Id = id.ToString(),
                        ParentId = parentId.ToString()
                    };
                    var response = SendOperationToControlSystem(finOperationInput, ControlSystemMethods.DoPay);
                    if (response.ResponseCode == ControlSystemSuccesResponseCode)
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        // Send BetShop finoperations to ControlSystem
        private ControlSystemResponseBase SendOperationToControlSystem(FinOperationInput input, string method)
        {
            try
            {
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = ConfigurationManager.AppSettings["ControlSystemPlatformGatewayUrl"] + method,
                    PostData = JsonConvert.SerializeObject(input)
                };
                var responseStr = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                return JsonConvert.DeserializeObject<ControlSystemResponseBase>(responseStr);
            }
            catch (Exception ex)
            {
                return new ControlSystemResponseBase { ResponseCode = ControlSystemGeneralErrorCode, Description = ex.Message };
            }
        }

        #endregion
    }
}
