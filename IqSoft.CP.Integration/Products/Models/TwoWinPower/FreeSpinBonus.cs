using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.TwoWinPower
{
    public class FreeSpinBonus
    {
        public int ProductId { get; set; }

      //  public string ProductExternalIdId { get; set; }

        public List<FreeSpin> FreeSpins { get; set; }
    }

    public class FreeSpin
    {
        public string Currency { get; set; }

        public decimal[] Denominations { get; set; }

        public List<FreeSpinBet> FreeSpinBets { get; set; }
    }

    public class FreeSpinBet
    {       
        public string Id { get; set; }

        public int Line { get; set; }
    }
}
