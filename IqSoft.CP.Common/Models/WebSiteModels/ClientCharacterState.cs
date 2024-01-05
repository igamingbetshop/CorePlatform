namespace IqSoft.CP.Common.Models.WebSiteModels
{
	public class ClientCharacterState
	{
		public ApiCharacter Parent { get; set; }
		public ApiCharacter Previous { get; set; }
		public ApiCharacter Next { get; set; }
		public decimal Current { get; set; }
	}
}