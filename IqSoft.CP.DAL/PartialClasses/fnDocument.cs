using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
  public partial class fnDocument : IBase
    {
        long IBase.ObjectId
        {
            get { return (long)Id; }
        }
        public decimal ConvertedAmount { get;set; }
    }
}
