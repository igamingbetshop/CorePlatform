﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class TriggerGroupSettingInfo
    {
        public int Id { get; set; }
        public int SettingId { get; set; }
        public int Order { get; set; }
    }
}
