using IqSoft.CP.Common.Attributes;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AgentModels
{
    public class ApiClientCorrections
    {
        public decimal? TotalAmount { get; set; }

        public long Count {get; set;}

        public List<ApiClientCorrection> Entities { get; set; }
    }

    public class ApiClientCorrection
    {
        public long Id { get; set; }

        public int AccoutTypeId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyId { get; set; }

        public int State { get; set; }

        public string Info { get; set; }

        public int? ClientId { get; set; }

        public int? UserId { get; set; }

        public System.DateTime CreationTime { get; set; }

        public System.DateTime LastUpdateTime { get; set; }

        public string OperationTypeName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [NotExcelProperty]

        public bool HasNote { get; set; }

        public string ClientUserName { get; set; }

        public string UserName { get; set; }

        [NotExcelProperty]
        public int? OperationTypeId { get; set; }

        [NotExcelProperty]
        public int? ProductId { get; set; }

        [NotExcelProperty]
        public string ProductNickName { get; set; }
    }
}