using IqSoft.CP.DataWarehouse.Interfaces;

namespace IqSoft.CP.DataWarehouse
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
