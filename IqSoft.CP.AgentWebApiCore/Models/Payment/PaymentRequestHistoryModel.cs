namespace IqSoft.CP.AgentWebApi.Models.Payment
{
    public class PaymentRequestHistoryModel
    {
        public long Id { get; set; }

        public long RequestId { get; set; }
        
        public int Status { get; set; }
        
        public string Comment { get; set; }
        
        public System.DateTime CreationTime { get; set; }

        public string FirstName {get; set;}

        public string LastName { get; set; }
    }
}