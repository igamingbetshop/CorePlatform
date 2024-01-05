﻿using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnReportByPartner : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return PartnerId; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Partner; }
        }
    }
}