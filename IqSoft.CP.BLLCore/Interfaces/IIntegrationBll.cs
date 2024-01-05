using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IIntegrationBll : IBaseBll
    {
        void SendWinsToControlSystem(BetShopFinOperationsOutput transactions);

        void SendPayToControlSystem(long id, long parentId);
    }
}
