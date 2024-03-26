namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class ApiCRMSetting
    {
        public int? Id { get; set; }
        public int PartnerId { get; set; }
        public string NickeName { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public int? MessageTemplateId { get; set; }
        public string Condition { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime FinishTime { get; set; }
        public int? Sequence { get; set; }
    }
}