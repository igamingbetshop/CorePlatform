using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnPaymentRequest : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnPaymentRequest; }
        }

        [NotMapped]
        public long? Barcode
        {
            get { return BetShopId == null ? null : (long?)(100000000000 + Id); }
        }
    }
}
