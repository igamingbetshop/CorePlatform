﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class GameProviderSetting
    {
        public long Id { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int GameProviderId { get; set; }
        public int State { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public Nullable<int> Order { get; set; }

        public virtual GameProvider GameProvider { get; set; }
        public virtual ObjectType ObjectType { get; set; }
    }
}