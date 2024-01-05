using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiExlusionModel
    {
        public int ClientId { get; set; }
        public string ToDate { get; set; }
        public int Type { get; set; }
    }
}