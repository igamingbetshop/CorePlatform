using System;

namespace IqSoft.CP.DAL.Models
{
    public class JobLog
    {
        public long Id { get; set; }
        public int JobId { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public int PeriodInSeconds { get; set; }
        public DateTime NextExecutionTime { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int Duration { get; set; }
        public string Message { get; set; }
    }
}
