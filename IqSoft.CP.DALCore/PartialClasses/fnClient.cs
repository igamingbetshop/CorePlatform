using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnClient : IBase
    {
        [NotMapped]
        public string SecurityQestionText { get; set; }

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
            set { EmailOrMobile = value; }
        }

        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Client; }
        }
    }
}
