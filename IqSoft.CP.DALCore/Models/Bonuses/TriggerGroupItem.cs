﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Bonuses
{
    public class TriggerGroupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Priority { get; set; }
        public List<TriggerSettingItem> TriggerSettings { get; set; }
    }
}