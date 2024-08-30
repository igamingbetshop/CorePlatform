namespace IqSoft.CP.DAL
{
	public partial class Character
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImageData { get; set; }
		public string BackgroundImageData { get; set; }
		public string MobileBackgroundImageData { get; set; }
		public string ItemBackgroundImageData { get; set; }
		public Character Copy()
		{
			return new Character
			{
				PartnerId = PartnerId,
				NickName = NickName,
				TitleTranslationId = TitleTranslationId,
				DescriptionTranslationId = DescriptionTranslationId,
				Status = Status,
				Order = Order,
				ImageUrl = ImageUrl,
				CompPoints = CompPoints,
				ParentId = ParentId
			};
		}
	}
}