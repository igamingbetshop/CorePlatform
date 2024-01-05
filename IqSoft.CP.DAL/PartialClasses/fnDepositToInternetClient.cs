using IqSoft.NGGP.Common;
using IqSoft.NGGP.DAL.Interfaces;
using Newtonsoft.Json;

namespace IqSoft.NGGP.DAL
{
    public partial class fnDepositToInternetClient : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return Id; }
        }

        [JsonIgnore]
        public int ObjectTypeId
        {
            get { return (int)Constants.ObjectTypes.fnDepositToInternetClient; }
        }
    }
}
