namespace IqSoft.CP.DAL.Models.Clients
{
	public class ClientCharacterState
	{
        public fnCharacter Previous { get; set; }
        public fnCharacter Next { get; set; }
        public decimal Current { get; set; }
    }
}
