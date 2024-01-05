﻿using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnClientIdentity : IBase
    {
        public long ObjectId
        {
            get { return ClientId; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Client; }
        }
    }
}