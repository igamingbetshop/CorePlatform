using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels.Menu
{
    public class ApiMenuItem
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string StyleType { get; set;}
        public string Href { get; set; }
		public bool OpenInRouting { get; set; }
		public bool Orientation { get; set; }
		public int Order { get; set; }

		public List<ApiSubMenuItem> SubMenu { get; set; }
	}
}
