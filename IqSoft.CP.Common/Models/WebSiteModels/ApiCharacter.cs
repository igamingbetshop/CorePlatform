namespace IqSoft.CP.Common.Models.WebSiteModels
{
	public class ApiCharacter
	{
		public int Id { get; set; }
		public int PartnerId { get; set; }
		public int? ParentId { get; set; }
		public string NickName { get; set; }
		public string ImageData { get; set; }
		public string BackgroundImageData { get; set; }
		public int Status { get; set; }
		public int Order { get; set; }
		public int? CompPoints { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}
}
