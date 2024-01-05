using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.ZeusPlay
{
    [XmlRoot("report")]
    public class Report
    {
        [XmlElement("open_sessions")]
        public ReportOpenSessions Item { get; set; }
    }

    public class ReportOpenSessions
    {
        [XmlElement("session")]
        public ReportOpenSessionsSession[] Session { get; set; }

        [XmlAttribute("date")]
        public string Date { get; set; }

        [XmlAttribute("period_secs")]
        public string PeriodSecs { get; set; }

        [XmlAttribute("timezone")]
        public string Timezone { get; set; }

        [XmlAttribute("random")]
        public string Random { get; set; }
    }

    public class ReportOpenSessionsSession
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("player_id")]
        public string PlayerId { get; set; }

        [XmlAttribute("sum_bets")]
        public string SumBets { get; set; }

        [XmlAttribute("sum_wins")]
        public string SumWins { get; set; }

        [XmlAttribute("count_bets")]
        public string CountBets { get; set; }

        [XmlAttribute("count_wins")]
        public string CountWins { get; set; }

        [XmlAttribute("currency")]
        public string Currency { get; set; }

        [XmlAttribute("datasig")]
        public string Datasig { get; set; }
    }
}