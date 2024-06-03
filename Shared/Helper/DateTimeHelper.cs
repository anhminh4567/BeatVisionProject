using Shared.RequestDto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helper
{
	public static class DateTimeHelper
	{
		public static DateTime FrontendTimePickerToLocalTime(DateTime dateTobeParsed) 
		{
			var publishDateUtc = DateTime.SpecifyKind(dateTobeParsed, DateTimeKind.Utc);
			var publishDateLocal = TimeZoneInfo.ConvertTimeFromUtc(publishDateUtc, TimeZoneInfo.Local);
			return publishDateLocal;
		}
		public static DateTime UtcTimeToLocalTime(string dateStringTobeParsed) 
		{
			DateTime parsedDateTime = DateTime.Parse(dateStringTobeParsed, null, DateTimeStyles.RoundtripKind);
			var publishDateUtc = parsedDateTime.ToUniversalTime();
			var publishDateLocal = TimeZoneInfo.ConvertTimeFromUtc(publishDateUtc, TimeZoneInfo.Local);
			return publishDateLocal;

		}
	}
}
