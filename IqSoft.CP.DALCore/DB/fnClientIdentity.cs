﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnClientIdentity
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int ClientId { get; set; }
        public string UserName { get; set; }
        public string ImagePath { get; set; }
        public int? UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int DocumentTypeId { get; set; }
        public int Status { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public long? ExpirationDate { get; set; }
        public bool? HasNote { get; set; }
    }
}
