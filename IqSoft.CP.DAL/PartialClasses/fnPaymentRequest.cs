using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnPaymentRequest : IBase
    {
        public decimal ConvertedAmount { get; set; }
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnPaymentRequest; }
        }

        public long? Barcode
        {
            get { return BetShopId == null ? null : (long?)(100000000000 + Id); }
        }
    }
}
