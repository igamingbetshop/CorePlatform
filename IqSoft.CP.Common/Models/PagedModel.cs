using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models
{
    public class PagedModel<T>
    {
        public long Count { get; set; }

        public IEnumerable<T> Entities { get; set; }
    }
}
