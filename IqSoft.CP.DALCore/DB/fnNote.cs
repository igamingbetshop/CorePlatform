﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnNote
    {
        public long Id { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public long SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string CreatorFirstName { get; set; }
        public string CreatorLastName { get; set; }
    }
}
