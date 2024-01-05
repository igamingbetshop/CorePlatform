﻿using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientSetting
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public long? NumericValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string StringValue { get; set; }

        public DateTime? LastUpdateTime { get; set; }
    }
}