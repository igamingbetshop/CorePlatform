using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.BlueOcean
{
    public class GameItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string type { get; set; }
        public string subcategory { get; set; }
        public string details { get; set; }
        public string system { get; set; }
        public string image { get; set; }
        public string image_filled { get; set; }
        public string image_background { get; set; }
        public bool play_for_fun_supported { get; set; }
        public bool mobile { get; set; }
        public bool freerounds_supported { get; set; }
        public bool has_jackpot { get; set; }
        public string currency { get; set; }
        public List<double> jackpotfeed { get; set; }
    }
}