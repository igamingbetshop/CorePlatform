﻿using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterBetShopGroup : ApiFilterBase
    {
        public int? Id { get; set; }

        public int? ParentId { get; set; }

        public int? PartnerId { get; set; }

        public bool? IsRoot { get; set; }

        public bool? IsLeaf { get; set; }

        public string Name { get; set; }

        public int? State { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}