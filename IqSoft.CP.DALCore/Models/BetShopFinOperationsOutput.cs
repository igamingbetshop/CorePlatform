using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class BetShopFinOperationsOutput : ResponseBase
    {
        public List<BetShopFinOperationDocument> Documents { get; set; }
    }
}
