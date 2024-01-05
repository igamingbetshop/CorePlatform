using System;

namespace IqSoft.CP.PaymentGateway.Models.JazzCashier
{
    public class PaymentInput
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public string IdInternalTransaction { get; set; }
        public long IdTransactionEx { get; set; }
        public long IdReference { get; set; }
        public Account Account { get; set; }
        public Transactionstatus TransactionStatus { get; set; }
        public Transactiontype TransactionType { get; set; }
        public Provider Provider { get; set; }
        public string Currency { get; set; }
        public Paymentmethod PaymentMethod { get; set; }
        public int Amount { get; set; }
        public int AmountUSD { get; set; }
        public string Description { get; set; }
        public string ConfimCode { get; set; }
        public bool NeedConfirmation { get; set; }
        public string UrlConfirmation { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Account
    {
        public int IdAccount { get; set; }
        public string Username { get; set; }
    }

    public class Transactionstatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Transactiontype
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Paymentmethod
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}