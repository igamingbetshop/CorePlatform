namespace IqSoft.CP.AdminWebApi.Models.ContentModels
{
    public class MessageTemplateModel
    {
        public int? Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public int ClientInfoType { get; set; }
        public string ExternalTemplateId { get; set; }
    }
}