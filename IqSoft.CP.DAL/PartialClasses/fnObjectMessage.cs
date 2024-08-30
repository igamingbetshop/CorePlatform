using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnClientMessage : IBase
    {
        public long ObjectId
        {
            get { return Id ?? 0; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.ClientMessage; }
        }
    }

    public partial class fnPartnerMessage : IBase
    {
        public long ObjectId
        {
            get { return Id ?? 0; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.ClientMessage; }
        }
    }

    public partial class fnAgentMessage : IBase
    {
        public long ObjectId
        {
            get { return Id ?? 0; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.ClientMessage; }
        }
    }

    public partial class fnAffiliateMessage : IBase
    {
        public long ObjectId
        {
            get { return Id ?? 0; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.ClientMessage; }
        }
    }
}
