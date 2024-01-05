using System.Collections.Generic;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IBetShopBll : IBaseBll
    {
        PagedModel<fnBetShops> GetBetShopsPagedModel(FilterfnBetShop filter);

        List<BetShop> GetBetShops(FilterBetShop filter, bool checkPermissions);

        PagedModel<fnCashDesks> GetCashDesksPagedModel(FilterfnCashDesk filter);

        List<CashDesk> GetCashDesks(FilterCashDesk filter);

        List<BetShopGroup> GetBetShopGroups(FilterBetShopGroup filter);

        BetShopGroup SaveBetShopGroup(BetShopGroup betShopGroup);
        
        BetShop SaveBetShop(BetShop betShop);

        CashDesk SaveCashDesk(CashDesk cashDesk);

        void DeleteBetShopGroup(int id);

        BetShop GetBetShopById(int id, bool checkPermission, int? userId = null);

        CashDeskShift CloseShift(IDocumentBll documentBl, IReportBll reportBl, int cashDeskId, int cashierId, long sessionId);

        void ChangeBetShopLimit(BetShop betShop, int userId);

        List<BetShop> GetBetShopsByClientId(int clientId);

		AdminShiftReportOutput GetfnAdminShiftReportPaging(FilterAdminShift filter);

        decimal UpdateShifts(IUserBll userBl, int cashierId, int cashDeskId);

        BetShopTicket GetBetShopBetByDocumentId(long documentId, bool isForPrint);

        void CashierIncasation(int cashDeskId, decimal amount);
    }
}
