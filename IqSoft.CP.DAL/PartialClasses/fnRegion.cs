﻿using System;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    [Serializable]
    public partial class  fnRegion : IBase
    {
        public long ObjectId
        {
            get
            {
                return Id;
            }
        }


        public int ObjectTypeId
        {
            get
            {
                return (int)ObjectTypes.fnRegion;
            }
        }
    }
}