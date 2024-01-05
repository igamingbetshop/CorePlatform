using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllMessageTemplate
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public string Text { get; set; }
        public int ClientInfoType { get; set; }
        public string ExternalTemplateId { get; set; }
    }
}
