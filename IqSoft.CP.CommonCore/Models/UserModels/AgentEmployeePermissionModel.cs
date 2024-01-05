namespace IqSoft.CP.Common.Models.UserModels
{
    public class AgentEmployeePermissionModel
    {
        public bool ViewBetsAndForecast { get; set; }
        public bool ViewReport { get; set; }
        public bool ViewBetsLists { get; set; }
        public bool ViewTransfer { get; set; }
        public bool ViewLog { get; set; }
        public int MemberInformationPermission { get; set; }
    }
}