namespace IqSoft.CP.DAL.Models
{
	public class NewPasswordInput
	{
		public int ClientId { get; set; }

        public string NewPassword { get; set; }

        public string Comment { get; set; }

        public int NotificationType { get; set; }
    }
}
