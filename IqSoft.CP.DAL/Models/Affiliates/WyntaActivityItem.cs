namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class WyntaActivityItem
    {
        public int Playerid { get; set; }
        public decimal Bonuses { get; set; }
        public decimal Chargebacks { get; set; }
        public decimal Deposits { get; set; }
        public decimal FirstDepositAmount { get; set; }
        public decimal Revenue { get; set; }

        /// <summary>
        /// Total bet amount
        /// </summary>
        public decimal Sidegamesbets { get; set; } 

        /// <summary>
        /// Total win amount
        /// </summary>
        public decimal Sidegameswins { get; set; } 
        public string WhitelabelId { get; set; }
        public string ClickID { get; set; }
        public string Date { get; set; }
        public string FirstDepositDate { get; set; }
    }
}
