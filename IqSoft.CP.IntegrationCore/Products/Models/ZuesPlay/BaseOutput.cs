using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Products.Models.ZuesPlay
{
    [XmlType("flash_game")]
    public class BaseOutput
    {
        [XmlElement("error_id")]
        public string error_id { get; set; }

        [XmlElement("success")]
        public bool success { get; set; }

        [XmlElement("game_html")]
        public string game_html { get; set; }
    }
}
