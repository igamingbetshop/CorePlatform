using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    [Serializable]
    public partial class fnOnlineClient : IBase
    {
        [JsonIgnore]
        [NotMapped]
        public long ObjectId
        {
            get { return Id ?? 0; }
        }


        [JsonIgnore]
        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnOnlineClient; }
        }
    }
}
