using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnReportByProduct : IBase
    {
        public long ObjectId
        {
            get { return ProductId; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnReportByProduct; }
        }
    }
}
