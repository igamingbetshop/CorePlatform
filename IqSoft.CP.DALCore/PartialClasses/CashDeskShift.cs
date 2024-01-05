using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class CashDeskShift
    {
        [NotMapped]
        public string CashierFirstName { get; set; }

        [NotMapped]
        public string CashierLastName { get; set; }

        [NotMapped]
        public int BetShopId { get; set; }

        [NotMapped]
        public string BetShopAddress { get; set; }
    }
}
