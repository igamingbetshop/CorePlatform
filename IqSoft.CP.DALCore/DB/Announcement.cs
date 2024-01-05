﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Announcement
    {
        public long Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public long Date { get; set; }
        public int ReceiverTypeId { get; set; }
        public Nullable<int> ReceiverId { get; set; }
        public long TranslationId { get; set; }

        public virtual ObjectType ReceiverType { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual Translation Translation { get; set; }
        public virtual User User { get; set; }
    }
}