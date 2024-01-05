using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
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
