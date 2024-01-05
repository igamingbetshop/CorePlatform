using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DistributionWebApi.Models.CModule
{
	public class InitGameOutput
	{
		public string method { get; set; }
		public string status { get; set; }
		public string message { get; set; }
		public object response { get; set; }
	}
}
