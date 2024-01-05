using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnAdminShiftReport : IBase
    {
        [JsonIgnore]
        [NotMapped]
        public long ObjectId
        {
            get { return BetShopId; }
        }

        [JsonIgnore]
        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.BetShop; }
        }
    }
}
