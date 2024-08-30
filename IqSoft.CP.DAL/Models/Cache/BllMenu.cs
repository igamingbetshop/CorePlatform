using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
	[Serializable]
	public class BllMenu
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public string StyleType { get; set; }
        public int? DeviceType { get; set; }
        public List<BllMenuItem> Items { get; set; }
	}
}
