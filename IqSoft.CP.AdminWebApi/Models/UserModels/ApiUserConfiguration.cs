using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public class ApiUserConfiguration
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public int? CreatedBy { get; set; }
        
        public string Name { get; set; }
        
        public bool? BooleanValue { get; set; }
        
        public decimal? NumericValue { get; set; }
        
        public string StringValue { get; set; }
        
        public DateTime CreationTime { get; set; }
        
        public DateTime LastUpdateTime { get; set; }
    }
}