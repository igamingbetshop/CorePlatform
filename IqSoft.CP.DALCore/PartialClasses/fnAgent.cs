using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnAgent: IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }
        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.User; }
        }
        [NotMapped]
        public decimal TotalBetAmount { get; set; }
        [NotMapped]
        public decimal DirectBetAmount { get; set; }
        [NotMapped]
        public decimal TotalWinAmount { get; set; }
        [NotMapped]
        public decimal DirectWinAmount { get; set; }
        [NotMapped]
        public decimal TotalGGR { get; set; }
        [NotMapped]
        public decimal DirectGGR { get; set; }
        [NotMapped]
        public decimal TotalTurnoverProfit { get; set; }
        [NotMapped]
        public decimal DirectTurnoverProfit { get; set; }
        [NotMapped]
        public decimal TotalGGRProfit { get; set; }
        [NotMapped]
        public decimal DirectGGRProfit { get; set; }

        [NotMapped]
        public List<int> ParentsPath { get; set; }
    }
}
