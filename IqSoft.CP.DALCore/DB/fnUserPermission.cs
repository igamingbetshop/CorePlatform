﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnUserPermission
    {
        public int UserId { get; set; }
        public string PermissionId { get; set; }
        public bool IsForAll { get; set; }
        public bool IsAdmin { get; set; }
    }
}
