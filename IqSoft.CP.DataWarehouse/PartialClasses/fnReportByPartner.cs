using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnReportByPartner : IBase
    {
        public long ObjectId
        {
            get { return PartnerId; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Partner; }
        }
    }
}
