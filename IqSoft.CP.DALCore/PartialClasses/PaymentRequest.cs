using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class PaymentRequest : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.PaymentRequest; }
        }

        [NotMapped]
        public decimal CashierBalance { get; set; }

        [NotMapped]
        public decimal ClientBalance { get; set; }

        [NotMapped]
        public decimal ObjectLimit { get; set; }

        [NotMapped]
        public string PaymentSystemName { get; set; }
    }
}
