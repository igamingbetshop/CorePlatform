using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.ProductGateway.Models.EkkoSpin
{
    public class WinnerInfo
    {
        public DocumentBll DocumentBll { get; set; }
        public BllClient Client { get; set; }
        public int ProviderId { get; set; }
        public int ProductId { get; set; }
        public int PartnerId { get; set; }
        public string TransactionId { get; set; }
        public int TransactionType { get; set; }
        public Document BetDocument { get; set; }
        public decimal WinAmount { get; set; }
    }
}