namespace IqSoft.NGGP.DAL.Models
{
    public class DashBoardResult
    {
        public int ActiveUsers { get; set; }

        public decimal TotalDeposit { get; set; }

        public decimal TotalWithdraw { get; set; }

        public decimal TotalTransferIn { get; set; }

        public decimal TotalTransferOut { get; set; }

        public decimal TotalBonus { get; set; }

        public decimal TotalCorrectionIn { get; set; }

        public decimal TotalCorrectionOut { get; set; }
    }
}
