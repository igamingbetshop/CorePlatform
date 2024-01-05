using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByUserTransaction
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string NickName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public int? FromUserId { get; set; }
        public string FromUsername { get; set; }
        public int? ClientId { get; set; }
        public string ClientUsername { get; set; }
        public int OperationTypeId { get; set; }
        public string OperationType { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}