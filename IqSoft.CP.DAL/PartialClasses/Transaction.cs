using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class Transaction : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)Common.Enums.ObjectTypes.Transaction; }
        }

		public int AccountTypeId { get; set; }
    }
}