using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnInternetGame : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return ProductId; }
        }

        [JsonIgnore]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Product; }
        }
    }
}
