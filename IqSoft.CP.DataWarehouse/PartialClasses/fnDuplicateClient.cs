using IqSoft.CP.DataWarehouse.Interfaces;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnDuplicateClient : IBase
    {
        public long ObjectId
        {
            get { return ClientId; }
        }

    }
}
