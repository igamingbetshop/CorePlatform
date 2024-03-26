namespace IqSoft.CP.TerminalManager.Models
{
    public class AppSetting : BaseSetting
    {
        public string Password { get; set; }
        public string Salt { get; set; }
        public string SerialNumber { get; set; }
    }
}
