namespace IqSoft.CP.AdminWebApi.Models.ProductModels
{
    public class ApiGameProviderSetting
    {
        public int ObjectId { get; set; }        
        public int GameProviderId { get; set; }
        public string GameProviderName { get; set; }
        public int State { get; set; }
        public int Order { get; set; }
    }
}