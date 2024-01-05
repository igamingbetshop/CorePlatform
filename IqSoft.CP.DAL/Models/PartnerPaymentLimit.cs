namespace IqSoft.CP.DAL.Models
{
    public class PartnerPaymentLimit
    {
        public int PartnerId { get; set; }

        public decimal? WithdrawMaxCountPerDayPerCustomer { get; set; }
               
        public decimal? CashWithdrawMaxCountPerDayPerCustomer { get; set; }
    }
}
