using IqSoft.CP.AgentWebApiCore;
using System.Threading.Tasks;

namespace IqSoft.CP.AgentWebApi.Helpers
{
    public static class Helpers
    {       
        public static void InvokeMessage(string messageName, params object[] obj)
        {
            Task.Run(() => Program.JobHubProxy.Invoke(messageName, obj));
        }
    }
}