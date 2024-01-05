namespace IqSoft.CP.Common.Models.CacheModels
{
	public class BllCharacter
	{
		public int Id { get; set; }
		public int PartnerId { get; set; }
		public int? ParentId{ get; set; }
		public string NickName { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImageData { get; set; }
		public string BackgroundImageData { get; set; }
		public int Status { get; set; }
		public int Order { get; set; }
		public int? CompPoints { get; set; }
	}
}
