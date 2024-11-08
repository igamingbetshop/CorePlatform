﻿using IqSoft.CP.Common.Models.AdminModels;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllPromotion
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public string ImageName { get; set; }
        public int State { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public BllSetting Segments { get; set; }
        public BllSetting Languages { get; set; }
        public int Order { get; set; }
        public int? ParentId { get; set; }
        public string StyleType { get; set; }
        public int? DeviceType { get; set; }
        public List<int> Visibility { get; set; }
    }
}
