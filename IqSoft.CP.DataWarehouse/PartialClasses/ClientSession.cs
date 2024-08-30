using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;

namespace IqSoft.CP.DataWarehouse
{
    public partial class ClientSession : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.ClientSession; }
        }
    }
}
