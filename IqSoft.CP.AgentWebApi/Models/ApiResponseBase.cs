﻿namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiResponseBase
    {
        public int ResponseCode { get; set; }
        public string Description { get; set; }
        public object ResponseObject { get; set; }
    }
}