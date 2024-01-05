using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnReportByPaymentSystem : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return 0; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.PaymentRequest; }
        }
    }
}