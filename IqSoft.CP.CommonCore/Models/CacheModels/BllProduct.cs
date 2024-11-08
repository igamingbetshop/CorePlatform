﻿using System;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllProduct
    {
        public int Id { get; set; }

        public long TranslationId { get; set; }

        public int? GameProviderId { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public string Description { get; set; }

        public int? ParentId { get; set; }

        public string ExternalId { get; set; }

        public int State { get; set; }

        public string MobileImageUrl { get; set; }

        public string WebImageUrl { get; set; }
        
        public string BackgroundImageUrl { get; set; }

        public int? SubProviderId { get; set; }

        public string NickName { get; set; }
        
        public bool? FreeSpinSupport { get; set; }
        
        public string Jackpot { get; set; }
        
        public bool HasDemo { get; set; }
    }
}
