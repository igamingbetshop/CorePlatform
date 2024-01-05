using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.Common.Models
{
    public class AMLStatus
    {
        public bool IsVerified { get; set; }
        public AMLStatuses Status { get; set; }
        public decimal Percentage { get; set; }
        public string Error { get; set; }
    }
}