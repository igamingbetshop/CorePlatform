using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using System.Threading.Tasks;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public static class PaymentHelpers
    {
        public static void RemoveClientBalanceFromCache(int clientId)
        {
            InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId));
        }

        public static void InvokeMessage(string messageName, params object[] obj)
        {
            Task.Run(() => Program.JobHubProxy.Invoke(messageName, obj));
        }

        public static ClientPaymentInfo RegisterClientPaymentAccountDetails(ClientPaymentInfo input)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                return clientBl.RegisterClientPaymentAccountDetails(input, null, false);
            }
        }
    }
}