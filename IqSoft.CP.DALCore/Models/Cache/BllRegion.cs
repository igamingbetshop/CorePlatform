using System;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class BllRegion
	{
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public int TypeId { get; set; }
		public string NickName { get; set; }
		public string Name { get; set; }
		public string IsoCode { get; set; }
		public string IsoCode3 { get; set; }
		public long TranslationId { get; set; }
		public string Path { get; set; }
		public int State { get; set; }
		public string LanguageId { get; set; }
	}
}
