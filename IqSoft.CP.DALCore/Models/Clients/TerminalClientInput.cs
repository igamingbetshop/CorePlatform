namespace IqSoft.CP.DAL.Models.Clients
{
    public class TerminalClientInput
    {
        public string TerminalId { get; set; }

        public int BetShopId { get; set; }

        public string AuthToken { get; set; }

        public int PartnerId { get; set; }

        public string Ip { get; set; }

        public string LanguageId { get; set; }
        public string Source { get; set; }
    }
}
