using System;
using System.Collections.Generic;
using System.Text;

namespace IqSoft.CP.Common.Models.CacheModels
{
    [Serializable]
    public class BllFnErrorType
    {
        public int Id { get; set; }

        public string NickName { get; set; }

        public long TranslationId { get; set; }

        public string Message { get; set; }

        public decimal? DecimalInfo { get; set; }

        public DateTime? DateTimeInfo { get; set; }

        public long? IntegerInfo { get; set; }
    }
}
