using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.SoftGaming
{
    public class GamesList
    {
        public List<GameItem> games { get; set; }
        public Dictionary<string, MerchantItem> merchants { get; set; }
    }
    public class GameItem
    {
        public string ID { get; set; }
        public string PageCode { get; set; }
        public string MobilePageCode { get; set; }
        public string HasDemo { get; set; }
        public string Freeround { get; set; }
        public int MerchantID { get; set; }
        public string ImageFullPath { get; set; }
        public List<int> CategoryID { get; set; }
        public Tranlations Name { get; set; }
    }
    public class Tranlations
    {
        public string en { get; set; }
    }

    public class MerchantItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}