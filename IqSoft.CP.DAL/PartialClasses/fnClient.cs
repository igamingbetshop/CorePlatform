using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnClient : IBase
    {
        public string SecurityQestionText { get; set; }

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
            set { EmailOrMobile = value; }
        }

        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Client; }
        }
    }
}
