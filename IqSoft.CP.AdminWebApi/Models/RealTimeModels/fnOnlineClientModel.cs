namespace IqSoft.CP.AdminWebApi.RealTimeModels.Models
{
    public class fnOnlineClientModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int RegionId { get; set; }
        public int? CategoryId { get; set; }
        public string LoginIp { get; set; }
        public int? SessionTime { get; set; }
        public int Balance { get; set; }
    }
}