using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnInternetGame : IBase
    {
        [JsonIgnore]
        [NotMapped]
        public long ObjectId
        {
            get { return ProductId; }
        }

        [JsonIgnore]
        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Product; }
        }
    }
}
