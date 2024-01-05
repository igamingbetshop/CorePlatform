using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;
using Newtonsoft.Json;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnInternetBet : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return BetDocumentId; }
        }

        [JsonIgnore]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnInternetBet; }
        }

        public decimal OriginalBetAmount { get; set; }
        public decimal OriginalWinAmount { get; set; }
        public decimal OriginalBonusAmount { get; set; }
    }
}
