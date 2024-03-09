using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
	public class ApiCharacter
	{
		public int Id { get; set; }
		public int PartnerId { get; set; }
		public int EnvironmentTypeId { get; set; }
		public string NickName { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImageData { get; set; }
		public string BackgroundImageData { get; set; }
		public string MobileBackgroundImageData { get; set; }
		public string ImageExtension { get; set; }
		public int Status { get; set; }
		public int Order { get; set; }
		public int? CompPoints { get; set; }
		public string SiteUrl { get; set; }
		public int? ParentId { get; set; }
	}
}