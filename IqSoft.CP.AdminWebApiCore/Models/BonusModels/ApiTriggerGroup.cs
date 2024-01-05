using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.BonusModels
{
    public class ApiTriggerGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BonusId { get; set; }
        public int Type { get; set; }
        public int Priority { get; set; }
    }
}