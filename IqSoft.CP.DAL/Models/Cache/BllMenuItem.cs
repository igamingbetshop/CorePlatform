using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllMenuItem
    {
        public int Id { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string StyleType { get; set;}
        public string Href { get; set; }
		public bool OpenInRouting { get; set; }
		public bool Orientation { get; set; }
		public int Order { get; set; }

		public List<BllSubMenuItem> SubMenu { get; set; }
	}
}
