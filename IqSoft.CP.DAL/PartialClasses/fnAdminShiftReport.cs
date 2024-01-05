using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;

namespace IqSoft.CP.DAL
{
    public partial class fnAdminShiftReport : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return BetShopId; }
        }

        [JsonIgnore]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.BetShop; }
        }
    }
}
