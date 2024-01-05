using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.OutcomeBet
{
    public class ActionInput
    {
        public string OPID { get; set; }
        public int CasinoId { get; set; }
        public int PlayerId { get; set; }
        public ContextModel Context { get; set; }
        public List<OperationModel> Operations { get; set; }
    }

    public class OperationModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Amount { get; set; }
    }
}