namespace IqSoft.CP.DAL.Models.User
{
    public class AgentDownlinesStatuses
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int TotalActive { get; set; }
        public int TotalSuspended { get; set; }
        public int TotalLocked { get; set; }
        public int TotalClosed { get; set; }
        public int TotalDisabled { get; set; }
        public int Unchanged { get; set; }
    }
}