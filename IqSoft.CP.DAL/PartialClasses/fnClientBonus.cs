using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using IqSoft.CP.DAL.Models.Bonuses;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class fnClientBonus : IBase
    {
        public long ObjectId
        {
            get { return ClientId; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.ClientBonus; }
        }
        public List<TriggerSettingItem> TriggerSettingItems { get; set; }
    }
}
