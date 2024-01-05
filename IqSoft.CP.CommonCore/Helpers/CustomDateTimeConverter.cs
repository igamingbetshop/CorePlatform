using Newtonsoft.Json.Converters;

namespace IqSoft.CP.Common.Helpers
{
	public class CustomDateTimeConverter : IsoDateTimeConverter
	{
		public CustomDateTimeConverter()
		{
			DateTimeFormat = "yyyy.MM.dd HH:mm:ss";
		}
	}
}
