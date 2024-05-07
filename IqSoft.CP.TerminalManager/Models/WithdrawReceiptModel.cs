namespace IqSoft.CP.TerminalManager.Models
{
    public class WithdrawReceiptModel : PrintInputBase
    {
        public string ShopAddress { get; set; }
        public string DeviceIdLabel { get; set; }
        public string DeviceId { get; set; }
        public string BranchIdLabel { get; set; }
        public string BranchId { get; set; }
        public string PrintDateLabel { get; set; }
        public string WithdrawIdLabel { get; set; }
        public string WithdrawId { get; set; }
        public string AmountLabel { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
        public string AttentionMessage { get; set; }
        public string Barcode { get; set; }
    }
}
