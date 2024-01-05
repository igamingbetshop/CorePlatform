using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllAccountType
    {
        public int Id { get; set; }

        public int Kind { get; set; }

        public bool CanBeNegative { get; set; }

        public long TranslationId { get; set; }

		public string NickName { get; set; }

        public string Name { get; set; }
    }
}
