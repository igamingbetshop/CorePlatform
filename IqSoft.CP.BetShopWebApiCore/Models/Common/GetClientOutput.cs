﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetClientOutput : ClientRequestResponseBase
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string CurrencyId { get; set; }
		public string UserName { get; set; }
		public int Gender { get; set; }
		public DateTime BirthDate { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string LastName { get; set; }
		public string SecondSurname { get; set; }
		public int? DocumentType { get; set; }
		public string DocumentNumber { get; set; }
		public string Address { get; set; }
		public string MobileNumber { get; set; }
		public string LanguageId { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime LastUpdateTime { get; set; }
		public List<object> Settings { get; set; }
	}
}