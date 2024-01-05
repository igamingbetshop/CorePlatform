using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnInternetBet : IBase
    {
        [JsonIgnore]
        [NotMapped]
        public long ObjectId
        {
            get { return BetDocumentId; }
        }

        [JsonIgnore]
        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnInternetBet; }
        }
        [NotMapped]
        public long Barcode
        {
            get { return CommonFunctions.CalculateBarcode(BetDocumentId); }
        }
    }
}
