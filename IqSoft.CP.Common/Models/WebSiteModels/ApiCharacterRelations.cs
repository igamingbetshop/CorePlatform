using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
	public class ApiCharacterRelations
	{
        public ApiCharacter Parent { get; set; }
        public List<ApiCharacter> Children { get; set; }
    }
}
