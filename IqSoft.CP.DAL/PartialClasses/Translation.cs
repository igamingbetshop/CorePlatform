using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class Translation : IBase
    {
        public long ObjectId
        {
            get
            {
                return Id;
            }
        }
    }
}