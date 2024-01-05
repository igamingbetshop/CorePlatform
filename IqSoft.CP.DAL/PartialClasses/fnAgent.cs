using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class fnAgent: IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.User; }
        }

        public decimal TotalBetAmount { get; set; }

        public decimal DirectBetAmount { get; set; }

        public decimal TotalWinAmount { get; set; }

        public decimal DirectWinAmount { get; set; }

        public decimal TotalGGR { get; set; }

        public decimal DirectGGR { get; set; }

        public decimal TotalTurnoverProfit { get; set; }

        public decimal DirectTurnoverProfit { get; set; }

        public decimal TotalGGRProfit { get; set; }

        public decimal DirectGGRProfit { get; set; }

        public List<int> ParentsPath { get; set; }
    }
}
