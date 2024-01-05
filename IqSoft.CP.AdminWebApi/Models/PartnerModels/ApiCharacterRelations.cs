using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
	public class ApiCharacterRelations
	{
		public ApiCharacter Parent { get; set; }
		public List<ApiCharacter> Children { get; set; }
	}
}