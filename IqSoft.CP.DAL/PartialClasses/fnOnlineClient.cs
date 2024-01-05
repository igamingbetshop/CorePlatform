using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.DAL
{
    [Serializable]
    public partial class fnOnlineClient : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return Id ?? 0; }
        }


        [JsonIgnore]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnOnlineClient; }
        }
    }
}
