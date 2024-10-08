using System;

namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class AffiliatePlatformModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public DateTime? LastExecutionTime { get; set; }
        public DateTime? KickOffTime { get; set; }
        public int? PeriodInHours { get; set; }
        public int? StepInHours { get; set; }
    }
}