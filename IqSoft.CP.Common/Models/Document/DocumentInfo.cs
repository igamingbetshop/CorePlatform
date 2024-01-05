namespace IqSoft.CP.Common.Models.Document
{
    public class DocumentInfo
    {
        public int BonusId { get; set; }
        public long ReuseNumber { get; set; }
        public bool FromBonusBalance { get; set; }
        public decimal BonusAmount { get; set; }
        public int? WinAccountTypeId { get; set; }
    }
}
