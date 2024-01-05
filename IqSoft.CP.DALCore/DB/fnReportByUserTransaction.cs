namespace IqSoft.CP.DAL
{
    using System;
    public partial class fnReportByUserTransaction
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string NickName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public Nullable<int> FromUserId { get; set; }
        public string FromUsername { get; set; }
        public Nullable<int> ClientId { get; set; }
        public string ClientUsername { get; set; }
        public int OperationTypeId { get; set; }
        public string OperationType { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public Nullable<long> Date { get; set; }
    }
}
