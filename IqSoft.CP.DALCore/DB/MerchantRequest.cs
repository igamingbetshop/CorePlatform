﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class MerchantRequest
    {
        public long Id { get; set; }
        public string RequestUrl { get; set; }
        public string Content { get; set; }
        public string Response { get; set; }
        public int Status { get; set; }
        public int RetryCount { get; set; }
    }
}