namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class TelegramUserData
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string hash { get; set; }
        public long auth_date { get; set; }
        public string photo_url { get; set; }
    }
}
