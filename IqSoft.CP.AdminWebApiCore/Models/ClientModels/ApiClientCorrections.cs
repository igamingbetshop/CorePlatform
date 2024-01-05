using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
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

        public int? OperationTypeId { get; set; }

        public System.DateTime CreationTime { get; set; }

        public System.DateTime LastUpdateTime { get; set; }

        public string OperationTypeName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool HasNote { get; set; }

        public int? ProductId { get; set; }

        public string ProductNickName { get; set; }
    }
}