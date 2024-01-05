using System;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiFreeSpin
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public bool Status { get; set; }
        public int ProductId { get; set; }      
        public int SpinsCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
    }
}