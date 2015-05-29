using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Utils
{
    public class TimeUtils
    {
        /// <summary>
        /// 脚本页面
        /// var timezoneOffset = new Date().getTimezoneOffset();
        /// $("#TimezoneOffset").val(timezoneOffset);
        /// </summary>
        public static string timeOffset;

        public static DateTime LocalToUtc(DateTime dt)
        {
            var Minutes1 = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours * 60;
            var Minutes2 = (int)LocalTimeZoneToMins();

            if (Minutes1 == Minutes2)
            {
                return dt.ToUniversalTime();
            }

            return dt == DateTime.MaxValue ? DateTime.Now.AddMinutes(Minutes1 - Minutes2).ToUniversalTime() : dt.AddMinutes(Minutes1 - Minutes2).ToUniversalTime();
        }

        public static DateTime UtcTimeToClientTime(DateTime? utcTime)
        {
            var dt = utcTime ?? DateTime.UtcNow;
            var datetime = GetClientDateTime(dt);
            return datetime;
        }

        public static string UtcToClient(DateTime? utcTime)
        {
            if (utcTime.HasValue)
            {
                var datetime = GetClientDateTime(utcTime.Value);
                return datetime.ToString("yyyy-MM-dd HH:mm");
            }
            return string.Empty;
        }

        public static string UtcTimeToClientTime(DateTime? utcTime, string format)
        {
            if (utcTime.HasValue)
            {
                var datetime = GetClientDateTime(utcTime.Value);
                return datetime.ToString(format);
            }
            return string.Empty;
        }

        public static string FormatDateTime(DateTime? datetime)
        {
            return FormatDateTime(datetime, null);
        }

        public static string FormatDateTime(DateTime? datetime, string formater)
        {
            if (string.IsNullOrEmpty(formater))
            {
                formater = "yyyy-MM-dd HH:mm";
            }
            if (datetime.HasValue)
            {
                return datetime.Value.ToString(formater);
            }
            else
            {
                return string.Empty;
            }
        }

        public static string UtcTimeToClientShortDate(DateTime? utcTime)
        {
            var strDate = string.Empty;
            if (utcTime.HasValue)
            {
                strDate = GetClientDateTime(utcTime.Value).ToString("yyyy-MM-dd");
            }
            return strDate;
        }

        public static string UtcTimeToClientShortTime(DateTime? utcTime)
        {
            var strDate = string.Empty;
            if (utcTime.HasValue)
            {
                strDate = GetClientDateTime(utcTime.Value).ToShortTimeString();
            }
            return strDate;
        }

        public static double LocalTimeZoneToMins()
        {
            //var timeOffset = Utility.CurrentLoginModel.ClientTimeZone;
            var hour = (int)(DataCast.Get<int>(timeOffset) / 60);
            var minute = (int)(DataCast.Get<int>(timeOffset) % 60);

            var prefix = "-";
            if (hour < 0 || minute < 0)
            {
                prefix = "+";
                hour = -hour;
                if (minute < 0)
                {
                    minute = -minute;
                }
            }
            if (prefix == "-")
            {
                hour = -hour;
            }

            return hour * 60 + (hour > 0 ? minute : -minute);
        }

        private static DateTime GetClientDateTime(DateTime dt)
        {
            try
            {
                //var timeOffset = Utility.CurrentLoginModel.ClientTimeZone;
                var hour = (int)(DataCast.Get<int>(timeOffset) / 60);
                var minute = (int)(DataCast.Get<int>(timeOffset) % 60);

                var prefix = "-";
                if (hour < 0 || minute < 0)
                {
                    prefix = "+";
                    hour = -hour;
                    if (minute < 0)
                    {
                        minute = -minute;
                    }
                }

                dt = dt.AddHours(DataCast.Get<int>(prefix + hour));
                dt = dt.AddMinutes(minute);

                return dt;
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public static DateTime GetLocalDate(string fromLocalTime)
        {
            DateTime time;
            DateTime.TryParse(fromLocalTime, out time);

            return time;
        }

        /// <summary>
        /// 把秒转换成分钟
        /// </summary>
        /// <returns></returns>
        public static int SecondToMinute(int second)
        {
            decimal mm = (decimal)((decimal)second / (decimal)60);
            return Convert.ToInt32(Math.Ceiling(mm));
        }

        /// <summary>
        /// 返回某年某月最后一天
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>日</returns>
        public static int GetMonthLastDate(int year, int month)
        {
            DateTime lastDay = new DateTime(year, month, new System.Globalization.GregorianCalendar().GetDaysInMonth(year, month));
            int Day = lastDay.Day;
            return Day;
        }

        /// <summary>
        ///  如果当前日期是周末，则向前推到不是周末的日期。
        /// </summary>
        /// <param name="date">需要推算的日期</param>
        /// <returns></returns>
        public static DateTime GetIsNotWeekEnd(DateTime date)
        {
            string str = date.DayOfWeek.ToString().ToUpper();
            if (str == "SUNDAY" || str == "SATURDAY")
                return GetIsNotWeekEnd(date.AddDays(-1));
            else
                return date;
        }

        /// <summary>
        /// 得到一段时间的工作日
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        public int GetWorkDays(DateTime start, DateTime end)
        {
            int workDays = 0;
            if (start.Year == end.Year && start.Month == end.Month)
            {
                workDays = GetSameYearAndMonthWorkDays(start, end);
            }
            else
            {
                if (start.Year == end.Year && start.Month != end.Month)
                {
                    workDays = GetSameYearDiffMonthWorkDays(start, end);
                }
                else
                {
                    workDays = GetDiffYearAndMonthWorkDays(start, end);
                }
            }
            return workDays;
        }

        /// <summary>
        /// 得到指定年月的工作日
        /// </summary>
        /// <param name="DigitTime">指定的时间</param>
        /// <returns></returns>
        public int GetDigitYearMonthWorkDays(DateTime DigitTime)
        {
            int Year = DigitTime.Year;
            int Month = DigitTime.Month;
            int days = DateTime.DaysInMonth(Year, Month);
            int count = days;
            for (int i = 0; i < days; i++)
            {
                string str = DigitTime.AddDays(i).DayOfWeek.ToString().ToUpper();
                if (str == "SUNDAY" || str == "SATURDAY")
                {
                    count -= 1;
                }
            }
            return count;
        }

        /// <summary>
        /// 得到同年同月的工作日
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        private int GetSameYearAndMonthWorkDays(DateTime startTime, DateTime endTime)
        {
            DateTime CurrStartTime = new DateTime(startTime.Year, startTime.Month, 1);
            int count = endTime.Day - startTime.Day + 1;
            for (int i = startTime.Day - 1; i < endTime.Day; i++)
            {
                string str = CurrStartTime.AddDays(i).DayOfWeek.ToString().ToUpper();
                if (str == "SUNDAY" || str == "SATURDAY")
                {
                    count -= 1;
                }
            }
            return count;
        }
        /// <summary>
        /// 得到同年不同月的工作日
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        private int GetSameYearDiffMonthWorkDays(DateTime startTime, DateTime endTime)
        {

            int startMonthDays = DateTime.DaysInMonth(startTime.Year, startTime.Month);
            int startWorkDays = GetSameYearAndMonthWorkDays(startTime, new DateTime(startTime.Year, startTime.Month, startMonthDays));

            int midWorkDays = 0;

            for (int i = startTime.Month + 1; i <= endTime.Month - 1; i++)
            {
                int days = GetDigitYearMonthWorkDays(new DateTime(startTime.Year, i, 1));
                midWorkDays += days;
            }

            int endWorkDays = GetSameYearAndMonthWorkDays(new DateTime(endTime.Year, endTime.Month, 1), endTime);

            return startWorkDays + midWorkDays + endWorkDays;
        }
        /// <summary>
        /// 得到不同年月的工作日
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        private int GetDiffYearAndMonthWorkDays(DateTime startTime, DateTime endTime)
        {
            int startWorkDays = 0;
            int endWorkDays = 0;
            int midWorkDays = 0;
            DateTime startEndTime = new DateTime(startTime.Year,
                                                 12,
                                                 DateTime.DaysInMonth(startTime.Year, 12)
                                                 );

            DateTime endStartTime = new DateTime(endTime.Year, 1, 1);

            if (startTime.Month == 12)
                startWorkDays = GetSameYearAndMonthWorkDays(startTime, startEndTime);
            else
                startWorkDays = GetSameYearDiffMonthWorkDays(startTime, startEndTime);

            if (endTime.Month == 1)
                endWorkDays = GetSameYearAndMonthWorkDays(endStartTime, endTime);
            else
                endWorkDays = GetSameYearDiffMonthWorkDays(endStartTime, endTime);

            int startYear = startTime.Year + 1;
            int count = endTime.Year;
            for (int i = startYear; i < count; i++)
            {
                int days = GetDigitYearWorkDays(new DateTime(i, 1, 1));
                midWorkDays += days;
            }

            return startWorkDays + midWorkDays + endWorkDays;
        }
        /// <summary>
        /// 得到指定年的工作日
        /// </summary>
        /// <param name="digitTime">指定时间</param>
        /// <returns></returns>
        public int GetDigitYearWorkDays(DateTime digitTime)
        {
            int totaldays = 0;
            int Year = digitTime.Year;
            for (int i = 1; i <= 12; i++)
            {
                DateTime digTime = new DateTime(Year, i, 1);
                int days = GetDigitYearMonthWorkDays(digTime);
                totaldays = days + totaldays;
            }
            return totaldays;
        }
    }
}
