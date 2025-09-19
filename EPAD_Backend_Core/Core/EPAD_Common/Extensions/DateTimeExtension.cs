using EPAD_Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPAD_Common.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime ConvertUnixTimeToDateTime(this string unixTime)
        {
            double ticks = double.Parse(unixTime);
            TimeSpan time = TimeSpan.FromMilliseconds(ticks);
            DateTime startdate = new DateTime(1970, 1, 1) + time;
            startdate = startdate.ToLocalTime();
            string s = startdate.ToString("yyyy-MM-dd HH:mm:ss");
            s = s + ".000";
            return DateTime.ParseExact(s, "yyyy-MM-dd HH:mm:ss.fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
        }
        public static DateTime FormatUnixTime(this DateTime unixTime)
        {
            string s = unixTime.ToString("yyyy-MM-dd HH:mm:ss");
            s = s + ".000";
            return DateTime.ParseExact(s, "yyyy-MM-dd HH:mm:ss.fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartDateOfMonth(this DateTime source)
        {
            return new DateTime(source.Year, source.Month, 1);
        }

        public static DateTime GetDateStart(this DateTime source, int? day)
        {
            int dayInMonth = DateTime.DaysInMonth(source.Year, source.Month);
            return new DateTime(source.Year, source.Month, day > dayInMonth ? dayInMonth : day ?? 1);
        }

        public static string ToDateString(this DateTime? source, string pFormat)
        {
            if (source.HasValue)
            {
                return source.Value.ToString(pFormat);
            }
            else
            {
                return "";
            }
        }

        public static DateTime ParseToTime(this DateTime source, DateTime date)
        {
            try
            {
                return new DateTime(source.Year, source.Month, source.Day, date.Hour, date.Minute, date.Second);
            }
            catch (Exception)
            {
                return source;
            }
        }

        public static long SafeGetLong(this SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt64(colIndex);
            return 0;
        }

        public static string SafeGetString(this SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }

        public static string ToHHmm(this DateTime source)
        {
            return source.ToString("HH:mm");
        }

        public static string ToHHmmss(this DateTime source)
        {
            return source.ToString("HH:mm:ss");
        }

        public static string ToddMMyyyyMinus(this DateTime source)
        {
            return source.ToString("dd-MM-yyyy");
        }

        public static string ToddMMyyyy(this DateTime source)
        {
            return source.ToString("dd/MM/yyyy");
        }

        public static string ToMMyyyy(this DateTime source)
        {
            return source.ToString("MM/yyyy");
        }

        public static string ToddMMyyyyHHmmssMinus(this DateTime source)
        {
            return source.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public static string ToyyyyMMddHHmmssMinus(this DateTime source)
        {
            return source.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToddMMyyyyHHmmss(this DateTime source)
        {
            return source.ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static string ToyyyyMMdd(this DateTime source)
        {
            return source.ToString("yyyy-MM-dd");
        }

        public static string ToyyyyMMddHHmmss(this DateTime source)
        {
            return source.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static bool CompareDate(this DateTime? sourceDate, DateTime? desDate)
        {
            var rs = sourceDate.Value.CompareTo(desDate.Value.Date);
            return rs > 0;
        }
        public static IEnumerable<DateTime> EachDay(this DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
                yield return day;
        }
        public static List<DateTime> GetListDate(DateTime start, DateTime end)
        { 
        return Enumerable.Range(0, 1 + end.Subtract(start).Days)
          .Select(offset => start.AddDays(offset))
          .ToList();
        }
        public static string GetDateOfWeekString(this DateTime source)
        {
            return (int)source.DayOfWeek != 0 ? "Thứ " + ((int)source.DayOfWeek + 1).ToString() + " (" + source.ToString("dd/MM/yyyy") + ")"
                    : "Chủ nhật (" + source.ToString("dd/MM/yyyy") + ")";
        }

        public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            daysToAdd = daysToAdd == 0 ? 7 : daysToAdd;
            return start.AddDays(daysToAdd);
        }

        public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
            dateTime.Kind);
        }

        public static bool AreTwoDateTimeRangesOverlapping(this TimeRangeParam incommingDateTimeRange, TimeRangeParam existingDateTimeRange)
        {
            return incommingDateTimeRange.DateStart < existingDateTimeRange.DateEnd && incommingDateTimeRange.DateEnd > existingDateTimeRange.DateStart;
        }

        public static bool AreManyDateTimeRangesOverlapping(this TimeRangeParam incommingDateTimeRange, List<TimeRangeParam> existingDateTimeRanges)
        {
            return existingDateTimeRanges.Any((existingDateTimeRange) => AreTwoDateTimeRangesOverlapping(incommingDateTimeRange, existingDateTimeRange));
        }
    }
}
