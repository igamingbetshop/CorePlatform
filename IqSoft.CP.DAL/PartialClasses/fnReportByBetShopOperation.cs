using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial  class fnReportByBetShopOperation : IBase
    {
        public long ObjectId
        {
            get
            {
                return Id;
            }
        }

        public int ObjectTypeId
        {
            get
            {
                return (int)ObjectTypes.fnReportByBetShopOperation;
            }
        }
    }
}
