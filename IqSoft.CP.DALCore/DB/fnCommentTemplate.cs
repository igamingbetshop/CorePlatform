﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnCommentTemplate
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
    }
}
