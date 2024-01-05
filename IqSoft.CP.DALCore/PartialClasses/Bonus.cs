using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Bonu : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Bonus; }
        }

        [NotMapped]
        public int GameProviderId { get; set; }

        [NotMapped]
        public int BetLine { get; set; }

        [NotMapped]
        public int Coins { get; set; }

        [NotMapped]
        public int GameId { get; set; }

        [NotMapped]
        public int Lines { get; set; }

        [NotMapped]
        public string Offer { get; set; }

        [NotMapped]
        public int Spins { get; set; }
    }
}
