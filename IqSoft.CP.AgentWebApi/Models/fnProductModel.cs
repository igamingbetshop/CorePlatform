namespace IqSoft.CP.AgentWebApi.Models
{
    public class FnProductModel
    {
        public int Id { get; set; }

        public int NewId { get; set; }

        public long TranslationId { get; set; }

        public int? GameProviderId { get; set; }

        public int? PaymentSystemId { get; set; }

        public int Level { get; set; }

        public string Description { get; set; }

        public int? ParentId { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string GameProviderName { get; set; }
        public int State { get; set; }
		public bool IsForDesktop { get; set; }
		public bool IsForMobile { get; set; }
		public int? SubproviderId { get; set; }
        public string WebImageUrl { get; set; }
        public string MobileImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
    }
}