using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class GetBalanceHistoryOutput : BaseResponse
    {
        public List<BalanceHistoryData> Data { get; set; }
    }
}