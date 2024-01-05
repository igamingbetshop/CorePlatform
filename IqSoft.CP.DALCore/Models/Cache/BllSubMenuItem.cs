using System;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class BllSubMenuItem
	{
		public int Id { get; set; }
		public string Icon { get; set; }
		public string Title { get; set; }
		public string Type { get; set; }
		public string Href { get; set; }
		public bool OpenInRouting { get; set; }
		public int Order { get; set; }
		public string StyleType { get; set; }
	}
}
