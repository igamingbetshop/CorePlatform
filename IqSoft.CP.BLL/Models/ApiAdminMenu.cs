using IqSoft.CP.DAL;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.BLL.Models
{
	public class ApiAdminMenu
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Color { get; set; }
		public string ApiRequest { get; set; }
		public string Route { get; set; }
		public string Path { get; set; }
		public int? ParentId { get; set; }
		public int Level { get; set; }
		
		[JsonIgnore]
		public string PermissionId { get; set; }
		public List<ApiAdminMenu> Pages { get; set; }
	}
}
