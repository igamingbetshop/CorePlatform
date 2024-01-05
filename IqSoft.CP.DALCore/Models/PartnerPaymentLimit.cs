namespace IqSoft.CP.DAL.Models
{
    public class PartnerPaymentLimit
    {
        public int PartnerId { get; set; }

        public long? WithdrawMaxCountPerDayPerCustomer { get; set; }
               
        public long? CashWithdrawMaxCountPerDayPerCustomer { get; set; }
    }
}
