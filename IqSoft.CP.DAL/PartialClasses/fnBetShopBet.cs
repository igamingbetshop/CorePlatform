using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;

namespace IqSoft.CP.DAL
{
    public partial class fnBetShopBet : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return BetDocumentId; }
        }

        [JsonIgnore]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnBetShopBet; }
        }

        public long Barcode { get; set; }
    }
}
