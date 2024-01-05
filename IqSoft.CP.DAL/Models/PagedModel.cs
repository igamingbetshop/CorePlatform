using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class PagedModel<T>
    {
        public long Count { get; set; }

        public IEnumerable<T> Entities { get; set; }
    }
}
