using System;
using Newtonsoft.Json.Converters;

namespace IqSoft.CP.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime GetUTCDateFromGmt(this DateTime date, double timeZone)
        {
			if (date == DateTime.MaxValue || date == DateTime.MinValue)
				return date;
			return date.AddHours(-timeZone);
        }

        public static DateTime? GetUTCDateFromGmt(this DateTime? date, double timeZone)
        {
            if (date.HasValue)
            {
				if (date == DateTime.MaxValue || date == DateTime.MinValue)
					return date;
				return date.Value.AddHours(-timeZone);
            }
            return null;
        }

        public static DateTime GetGMTDateFromUTC(this DateTime date, double timeZone)
        {
            if (date == DateTime.MaxValue || date == DateTime.MinValue)
                return date;
            return date.AddHours(timeZone);
        }

        public static DateTime? GetGMTDateFromUTC(this DateTime? date, double timeZone)
        {
            if (date == null || date == DateTime.MaxValue || date == DateTime.MinValue)
				return date;
            return date.Value.AddHours(timeZone);
        }

        public class CustomDateTimeConverter : IsoDateTimeConverter
        {
            public CustomDateTimeConverter()
            {
                DateTimeFormat = "yyyy.MM.dd HH:mm:ss";
            }
        }
    }
}