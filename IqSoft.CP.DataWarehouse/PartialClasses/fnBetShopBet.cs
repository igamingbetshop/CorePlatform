﻿using IqSoft.CP.Common.Enums;
using IqSoft.CP.DataWarehouse.Interfaces;
using Newtonsoft.Json;

namespace IqSoft.CP.DataWarehouse
{
    public partial class fnBetShopBet : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return BetDocumentId; }
        }

        [JsonIgnore]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnBetShopBet; }
        }

        public long Barcode { get; set; }
    }
}
