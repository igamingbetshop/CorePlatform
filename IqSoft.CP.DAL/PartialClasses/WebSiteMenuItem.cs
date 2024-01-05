﻿using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class WebSiteMenuItem : IBase
    {
        public int PartnerId { get; set; }

        public string Image { get; set; }

        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId { get; set; } = (int)ObjectTypes.WebSiteMenuItem;
    }
}