﻿namespace IqSoft.CP.Integration.Products.Models.SunCity
{
    public class OuthOutput
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }

    }
}
