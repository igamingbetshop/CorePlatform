using System;

namespace IqSoft.CP.PaymentGateway.Models.SDPay
{
    public class PayoutInput
    {
        public string HiddenField1 { get; set; }
    }

    public class TransferInfomation
    {
      public int Id { get; set; }
       
      public string RolloutAccount { get; set; }
       
      public string IntoAccount { get; set; }
       
      public string IntoName { get; set; }
       
      public string IntoBank1 { get; set; }
       
      public string IntoAmount { get; set; }
       
      public int RecordsState { get; set; }
       
      public string Tip { get; set; }
       
      public DateTime ApplicationTime { get; set; }
       
      public DateTime ProcessingTime { get; set; }
       
      public string SerialNumber { get; set; }
       
      public decimal beforeMoney { get; set; }
       
      public string afterMoney { get; set; }
       
      public string bankNumber { get; set; }
    }
}