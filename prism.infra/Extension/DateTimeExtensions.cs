using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Extension
{
    public static class DateTimeExtensions
    {
        public static string ToStringWorkWeek(this DateTime date, string workweekPrefix="WW", char seperator = '-', DayOfWeek startOfWeek = DayOfWeek.Monday, Boolean isAddYear = false)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var week = calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, startOfWeek);
            return isAddYear ? $"{date.Year}{seperator}{workweekPrefix}{week:D2}" : $"{workweekPrefix}{week:D2}";
        }

        public static DateTime FromStringWorkWeek(string workweekString, string workweekPrefix = "WW", char seperator = '-', DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            if (!workweekString.Contains(workweekPrefix))
                throw new ArgumentException($"Input string must contains the prefix '{workweekPrefix}'.");
            var weekPart = workweekString.Substring(workweekString.IndexOf(workweekPrefix) + workweekPrefix.Length);
            var yearPart = workweekString.Substring(0, workweekString.IndexOf(workweekPrefix)).TrimEnd(seperator);
            if (!int.TryParse(weekPart, out int weekNumber))
                throw new ArgumentException("Invalid week number in the input string.");

            var year = int.TryParse(yearPart, out int parsedYear) ? parsedYear : DateTime.Now.Year;
            var firstDayOfYear = new DateTime(year, 1, 1);
            var daysOffset = (int)startOfWeek - (int)firstDayOfYear.DayOfWeek;
            var firstWeekStart = firstDayOfYear.AddDays(daysOffset);
            return firstWeekStart.AddDays((weekNumber - 1) * 7);
        }
    }
}
