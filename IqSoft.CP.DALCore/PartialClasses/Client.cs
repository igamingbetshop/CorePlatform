using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Client : IBase
    {
        [NotMapped]
        public string Token { get; set; }

        [NotMapped]
        public string Password { get; set; }

        [NotMapped]
        public string EmailOrMobile
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Email))
                    return Email;
                return MobileNumber;
            }
        }

        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int) ObjectTypes.Client; }
        }

        [NotMapped]
        public string Comment { get; set; }

        [NotMapped]
        public string WelcomeBonusActivationKey { get; set; }

        [NotMapped]
        public string CurrencySymbol { get; set; }

        [NotMapped]
        public List<int> ParentsPath { get; set; }
	}
}