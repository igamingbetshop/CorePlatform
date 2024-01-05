using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class ResetBetShopDailyTicketNumberOutput
    {
        public List<ResetBetShopDailyTicketNumberOutputItem> Results { get; set; }
    }

    public class ResetBetShopDailyTicketNumberOutputItem
    {
        public int PartnerId { get; set; }

        public bool ResetResult { get; set; }
    }
}
