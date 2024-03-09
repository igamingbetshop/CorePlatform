using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllPopup
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public long ContentTranslationId { get; set; }
        public string ImageName { get; set; }
        public int Order { get; set; }
        public string Page { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public List<int> SegmentIds { get; set; }
        public List<int> ClientIds { get; set; }
    }
}