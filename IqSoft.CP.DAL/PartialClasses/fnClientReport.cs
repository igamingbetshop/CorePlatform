﻿using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnClientReport : IBase
    {
        public long ObjectId
        {
            get { return ClientId.Value; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.fnClientReport; }
        }
    }
}
