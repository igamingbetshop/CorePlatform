using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnClientReport : IBase
    {
        public long ObjectId
        {
            get { return ClientId; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnClientReport; }
        }
    }
}
