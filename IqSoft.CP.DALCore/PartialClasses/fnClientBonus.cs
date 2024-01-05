using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using IqSoft.CP.DAL.Models.Bonuses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnClientBonus : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return ClientId; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.ClientBonus; }
        }
        [NotMapped]
        public List<TriggerSettingItem> TriggerSettingItems { get; set; }
    }
}
