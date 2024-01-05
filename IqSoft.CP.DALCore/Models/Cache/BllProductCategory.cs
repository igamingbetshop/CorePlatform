using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class BllProductCategory
	{
		public int Id { get; set; }
		public string Nickname { get; set; }
		public string Name { get; set; }
		public int Type { get; set; }
		public long TranslationId { get; set; }

		public List<int> Products { get; set; }
	}
}
