using System;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiUserCorrections
    {
        public decimal? TotalAmount { get; set; }

        public long Count { get; set; }

        public List<ApiUserCorrection> Entities { get; set; }
    }

    public class ApiUserCorrection
    {
        public long Id { get; set; }

        public int AccoutTypeId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyId { get; set; }

        public int State { get; set; }

        public string Info { get; set; }

        public int? FromUserId { get; set; }

        public int? UserId { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string OperationTypeName { get; set; }

        public string CreatorFirstName { get; set; }
        public string CreatorLastName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }

        public bool HasNote { get; set; }
    }
}