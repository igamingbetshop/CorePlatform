using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class User : IBase
    {
        public string Password { get; set; }

        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.User; }
        }

        public int? OddsType { get; set; }
        public decimal? CorrectionMaxAmount { get; set; }
        public string CorrectionMaxAmountCurrency { get; set; }
    }
}
