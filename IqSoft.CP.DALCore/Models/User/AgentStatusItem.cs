namespace IqSoft.CP.DAL.Models.User
{
    public class AgentStatusItem
    {
        public int State { get; set; }
        public int? ParentState { get; set; }
        public ClientSetting ClientParentState { get; set; }
        public string Path { get; set; }
    }
}
