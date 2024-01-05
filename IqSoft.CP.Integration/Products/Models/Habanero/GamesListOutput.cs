using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Habanero
{
    public class GamesListOutput
    {
        public List<Game> Games { get; set; }
    }

    public class Game
    {
        public string BrandGameId { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public bool IsNew { get; set; }
        public DateTime DtAdded { get; set; }
        public DateTime DtUpdated { get; set; }
        public int GameTypeId { get; set; }
        public int ReleaseStatusId { get; set; }
        public string GameTypeName { get; set; }
        public bool MobileCapable { get; set; }
        public int MobiPlatformId { get; set; }
        public int WebPlatformId { get; set; }
        public string GameTypeDisplayName { get; set; }
        public int BaseGameTypeId { get; set; }
        public bool ExProv { get; set; }
        public string ProductExternalID { get; set; }
        public string LineDesc { get; set; }
        public bool IsFeat { get; set; }
        public float RTP { get; set; }
        public string ReportName { get; set; }
        public bool SupportBonusFS { get; set; }
       // public Translatedname[] TranslatedNames { get; set; }
        public DateTime DtRTM { get; set; }
    }

    public class Translatedname
    {
        public int LanguageId { get; set; }
        public string Locale { get; set; }
        public string Translation { get; set; }
    }

}
