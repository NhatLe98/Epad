using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EPAD_Common.Extensions
{
    public static class StringExtensions
    {
        public static string GetNormalATID(this string pValue)
        {
            return pValue.PadLeft(GlobalParams.MAX_LENGTH_ATID, '0');
        }

        public static DateTime TryGetDateTime(this string pValue, string pFormat = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.ParseExact(pValue, pFormat, CultureInfo.InvariantCulture);
        }

        public static string GetInOutModeString(this short inOutMode)
        {
            string value = "Other";
            switch (inOutMode)
            {
                case 0:
                    value = "In"; break;
                case 1:
                    value = "Out"; break;
                case 2:
                    value = "BreakOut"; break;
                case 3:
                    value = "BreakIn"; break;
            }
            return value;
        }

        public static string GetGCSInOutModeString(this short inOutMode)
        {
            string value = "Other";
            switch (inOutMode)
            {
                case 1:
                    value = "In"; break;
                case 2:
                    value = "Out"; break;
            }
            return value;
        }

        public static string GetFaceMaskString(this int? faceMask)
        {
            string value = "";
            if (faceMask != null)
            {
                switch (faceMask)
                {
                    case 0:
                        value = "NoFaceMask"; break;
                    case 1:
                        value = "HaveFaceMask"; break;
                    case 255:
                        value = ""; break;
                }
            }
            return value;
        }

        public static string GetVehicleTypeName(this short type)
        {
            string value = "";
            switch (type)
            {
                case 0:
                    value = "Xe máy"; break;
                case 1:
                    value = "Xe đạp"; break;
                case 2:
                    value = "Xe đạp điện"; break;
                case 3:
                    value = "Xe ô tô"; break;
                default:
                    value = "Không có phương tiện";
                    break;
            }
            return value;
        }

        public static string GetBodyTemperatureString(this double? bodyTemperature)
        {
            string value = "";
            if (bodyTemperature != null)
            {
                switch (bodyTemperature)
                {
                    case 255:
                        value = "";
                        break;
                    default:
                        value = bodyTemperature.ToString();
                        break;
                }
            }
            return value;
        }

        public static string GetVerifyModeString(this short? verifyMode)
        {
            string value = "Other";
            switch (verifyMode)
            {
                case 1:
                    value = "Finger"; break;
                case 2:
                    value = "PIN"; break;
                case 3:
                    value = "Password"; break;
                case 4:
                    value = "Card"; break;
                case 15:
                    value = "FaceTemplate"; break;
            }
            return value;
        }

        public static short GetVerifyModeFromString(this string verifyMode)
        {
            short value = 0;
            switch (verifyMode)
            {
                case "Vân tay":
                    value = 1; break;
                case "Pin":
                    value = 2; break;
                case "Mật khẩu":
                    value = 3; break;
                case "Thẻ":
                    value = 4; break;
                case "Khuôn mặt":
                    value = 15; break;
            }
            return value;
        }

        public static string ConvertToUnSign3(this string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
                string temp = s.Normalize(NormalizationForm.FormD);
                return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
            }
            else {
                return "";
            }
        }

        public static string ConvertToUnSign2(this string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }

        public static bool ContainsIgnoreCase(this string pSource, string pVal)
        {
            if (pVal == null) return true;
            return pSource.Contains(pVal, StringComparison.OrdinalIgnoreCase);
        }

        public static int GetWeekOfYear(this DateTime time)
        {
            var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public static string RemoveDiacritics(this string accentedString)
        {
            string normalizedString = accentedString.Normalize(NormalizationForm.FormKD);
            string unsignedString = string.Empty;

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    unsignedString += c;
                }
            }

            return unsignedString;
        }

        public static string ConvertACTimeToString(this string time)
        {
            if(string.IsNullOrEmpty(time)) return "";
            return time.Insert(2, ":");
        }

        public static List<string> SplitByComma(this string time)
        {
            if (!string.IsNullOrWhiteSpace(time) && !time.All(x => x == ','))
            {
                return time.Split(',').ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
