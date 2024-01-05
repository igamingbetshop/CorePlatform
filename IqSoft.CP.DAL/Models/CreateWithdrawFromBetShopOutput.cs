using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class CreateWithdrawsFromBetShopOutput
    {
        public List<DAL.Document> Documents { get; set; }
        public string TicketNumber { get; set; }
    }
}
