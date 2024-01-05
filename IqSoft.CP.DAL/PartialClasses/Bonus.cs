using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class Bonu : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Bonus; }
        }

        public int GameProviderId { get; set; }

        public int BetLine { get; set; }

        public int Coins { get; set; }

        public int GameId { get; set; }

        public int Lines { get; set; }

        public string Offer { get; set; }

        public int Spins { get; set; }
    }
}
