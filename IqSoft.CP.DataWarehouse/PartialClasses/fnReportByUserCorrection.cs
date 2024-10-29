using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnReportByUserCorrection : IBase
    {
        public long ObjectId
        {
            get { return UserId; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnCorrection; }
        }
    }
}
