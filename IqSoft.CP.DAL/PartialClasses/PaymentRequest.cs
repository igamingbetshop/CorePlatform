using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class PaymentRequest : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.PaymentRequest; }
        }

        public decimal CashierBalance { get; set; }

        public decimal ClientBalance { get; set; }

        public decimal ObjectLimit { get; set; }

        public string PaymentSystemName { get; set; }
        public bool BonusRefused { get; set; }
    }
}
