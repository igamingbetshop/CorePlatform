using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Popup : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Popup; }
        }

        public List<int> SegmentIds { get; set; }
        public List<int> ClientIds { get; set; }
    }
}