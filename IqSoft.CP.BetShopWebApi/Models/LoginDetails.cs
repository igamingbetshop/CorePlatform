﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models
{
	public class LoginDetails
	{
		public string UserName { get; set; }

		public string Password { get; set; }

		public string Hash { get; set; }
	}
}