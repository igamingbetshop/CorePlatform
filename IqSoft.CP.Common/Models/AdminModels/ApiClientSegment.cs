namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiClientSegment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public int State { get; set; }
        public int Mode { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public System.DateTime ConsideredDate { get; set; }
    }
}
