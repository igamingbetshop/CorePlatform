using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Client : IBase
    {
        public string Token { get; set; }

        public string Password { get; set; }

        public string EmailOrMobile
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Email))
                    return Email;
                return MobileNumber;
            }
        }

        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int) ObjectTypes.Client; }
        }

        public string Comment { get; set; }

        public string WelcomeBonusActivationKey { get; set; }
		
        public string CurrencySymbol { get; set; }
        public string PinCode { get; set; }
        public bool? Duplicated { get; set; }

        public List<int> ParentsPath { get; set; }
	}
}