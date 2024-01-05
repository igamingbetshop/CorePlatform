using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnReportByProvider : IBase
    {
        public long ObjectId
        {
            get { return 0; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnReportByProvider; }
        }
    }
}
