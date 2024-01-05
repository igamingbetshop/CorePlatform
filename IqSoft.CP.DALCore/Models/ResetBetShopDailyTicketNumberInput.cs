using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class ResetBetShopDailyTicketNumberInput
    {
        public List<DailyTicketNumberResetSetting> Settings { get; set; }
    }

    public class DailyTicketNumberResetSetting
    {
        public DateTime ResetTime { get; set; }
        public int PartnerId { get; set; }
    }
}
