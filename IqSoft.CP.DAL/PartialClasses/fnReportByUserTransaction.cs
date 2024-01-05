using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnReportByUserTransaction : IBase
    {
        public long ObjectId
        {
            get { return UserId; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.User; }
        }
    }
}
