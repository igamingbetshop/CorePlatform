using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnActionLog : IBase
    {
        long IBase.ObjectId
        {
            get { return (long)ObjectId; }
        }
    }
}
