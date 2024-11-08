﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ClientClassification
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? State { get; set; }
        public int? CategoryId { get; set; }
        public int ProductId { get; set; }
        public long? SessionId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int? SegmentId { get; set; }

        public virtual ClientCategory Category { get; set; }
        public virtual Client Client { get; set; }
        public virtual Product Product { get; set; }
        public virtual Segment Segment { get; set; }
    }
}