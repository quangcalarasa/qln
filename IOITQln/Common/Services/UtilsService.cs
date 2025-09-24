using DevExpress.CodeParser.VB;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IOITQln.Common.Services
{
    public class UtilsService
    {
        public static readonly string partternPassword = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{7,}$";
        public static readonly string errMsgPartternPassword = "Mật khẩu tối thiểu 8 kí tự, phải có 1 chữ thường, 1 chữ hoa, 1 số và 1 ký tự đặc biệt!";

        public static string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }
        public static string ConvertPercentToString(string percent)
        {
            string[] strArray1 = new string[6]
            {
    "",
    "mươi",
    "trăm",
    "nghìn",
    "triệu",
    "tỉ"
            };
            string[] strArray2 = new string[10]
            {
    "không",
    "một",
    "hai",
    "ba",
    "bốn",
    "năm",
    "sáu",
    "bảy",
    "tám",
    "chín"
            };
            string str1 = percent.Split('.', StringSplitOptions.None)[0];
            int length = str1.Length;
            string str2 = str1 + "ss";
            string str3 = "";
            int num1 = 0;
            int num2;
            for (int index1 = 0; index1 < length; index1 += num2)
            {
                num2 = (length - index1 + 2) % 3 + 1;
                int num3 = 0;
                for (int index2 = 0; index2 < num2; ++index2)
                {
                    if (str2[index1 + index2] != '0')
                    {
                        num3 = 1;
                        break;
                    }
                }
                if (num3 == 1)
                {
                    num1 = 1;
                    for (int index3 = 0; index3 < num2; ++index3)
                    {
                        int num4 = 1;
                        switch (str2[index1 + index3])
                        {
                            case '0':
                                if (num2 - index3 == 3)
                                    str3 = $"{str3}{strArray2[0]} ";
                                if (num2 - index3 == 2)
                                {
                                    if (str2[index1 + index3 + 1] != '0')
                                        str3 += "lẻ ";
                                    num4 = 0;
                                    break;
                                }
                                break;
                            case '1':
                                if (num2 - index3 == 3)
                                    str3 = $"{str3}{strArray2[1]} ";
                                if (num2 - index3 == 2)
                                {
                                    str3 += "mười ";
                                    num4 = 0;
                                }
                                if (num2 - index3 == 1)
                                {
                                    int index4 = index1 + index3 != 0 ? index1 + index3 - 1 : 0;
                                    str3 = str2[index4] == '1' || str2[index4] == '0' ? $"{str3}{strArray2[1]} " : str3 + "mốt ";
                                    break;
                                }
                                break;
                            case '5':
                                str3 = index1 + index3 != length - 1 ? $"{str3}{strArray2[5]} " : str3 + "lăm ";
                                break;
                            default:
                                str3 = $"{str3}{strArray2[(int)str2[index1 + index3] - 48 /*0x30*/]} ";
                                break;
                        }
                        if (num4 == 1)
                            str3 = $"{str3}{strArray1[num2 - index3 - 1]} ";
                    }
                }
                if (length - index1 - num2 > 0)
                {
                    if ((length - index1 - num2) % 9 == 0)
                    {
                        if (num1 == 1)
                        {
                            for (int index5 = 0; index5 < (length - index1 - num2) / 9; ++index5)
                                str3 += "tỉ ";
                        }
                        num1 = 0;
                    }
                    else if (num3 != 0)
                        str3 = $"{str3}{strArray1[(length - index1 - num2 + 1) % 9 / 3 + 2]} ";
                }
            }
            return length == 1 && (str2[0] == '0' || str2[0] == '5') ? strArray2[(int)str2[0] - 48 /*0x30*/].ToUpper() + " phần trăm" : Regex.Replace(str3 + " phần trăm", "\\s+", " ");
        }
        public static void MergeCellRanges(ISheet sheet, params CellRangeAddress[] ranges)
        {
            foreach (CellRangeAddress range in ranges)
            {
                sheet.AddMergedRegion(range);
            }
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int GetWeekInYear(DateTime time)
        {
            CultureInfo myCI = CultureInfo.CurrentCulture;
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;

            return myCal.GetWeekOfYear(time, myCWR, myFirstDOW);
        }

        public static string ColumnAdress(int col)
        {
            if (col <= 26)
            {
                return Convert.ToChar(col + 64).ToString();
            }
            int div = col / 26;
            int mod = col % 26;
            if (mod == 0) { mod = 26; div--; }
            return ColumnAdress(div) + ColumnAdress(mod);
        }

        public static string ConvertRomanNumber(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("Value must be between 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ConvertRomanNumber(number - 1000);
            if (number >= 900) return "CM" + ConvertRomanNumber(number - 900); //EDIT: i've typed 400 instead 900
            if (number >= 500) return "D" + ConvertRomanNumber(number - 500);
            if (number >= 400) return "CD" + ConvertRomanNumber(number - 400);
            if (number >= 100) return "C" + ConvertRomanNumber(number - 100);
            if (number >= 90) return "XC" + ConvertRomanNumber(number - 90);
            if (number >= 50) return "L" + ConvertRomanNumber(number - 50);
            if (number >= 40) return "XL" + ConvertRomanNumber(number - 40);
            if (number >= 10) return "X" + ConvertRomanNumber(number - 10);
            if (number >= 9) return "IX" + ConvertRomanNumber(number - 9);
            if (number >= 5) return "V" + ConvertRomanNumber(number - 5);
            if (number >= 4) return "IV" + ConvertRomanNumber(number - 4);
            if (number >= 1) return "I" + ConvertRomanNumber(number - 1);
            throw new ArgumentOutOfRangeException("Value must be between 1 and 3999");
        }

        public static int GetWeekOrderInYear(DateTime time)
        {
            CultureInfo myCI = CultureInfo.CurrentCulture;
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;

            return myCal.GetWeekOfYear(time, myCWR, myFirstDOW);
        }

        public static int GetQuarter(int month)
        {
            int quarter = 1;
            if (month >= 1 && month <= 3)
                quarter = 1;
            if (month >= 4 && month <= 6)
                quarter = 2;
            if (month >= 7 && month <= 9)
                quarter = 3;
            if (month >= 10 && month <= 12)
                quarter = 4;
            return quarter;
        }

        public static DateTime ConvertDateDDMMYYYYStart(string datetime)
        {
            DateTime res = new DateTime(1990, 1, 1);
            string[] td1 = datetime.Split(' ');
            string[] td = td1[0].Split('-');
            try
            {
                res = new DateTime(int.Parse(td[0]), int.Parse(td[1]), int.Parse(td[2]), 0, 0, 0);
            }
            catch
            {
                res = new DateTime(1990, 1, 1);
            }


            return res;
        }

        public static DateTime ConvertDateDDMMYYYYEnd(string datetime)
        {
            DateTime res = new DateTime(1990, 1, 1);
            string[] td1 = datetime.Split(' ');
            string[] td = td1[0].Split('-');
            try
            {
                res = new DateTime(int.Parse(td[0]), int.Parse(td[1]), int.Parse(td[2]), 23, 59, 59);
            }
            catch
            {
                res = new DateTime(1990, 1, 1);
            }


            return res;
        }

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            // Use first Thursday in January to get first week of the year as
            // it will never be in Week 52/53
            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            // As we're adding days to a date in Week 1,
            // we need to subtract 1 in order to get the right date for week #1
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            // Using the first Thursday as starting week ensures that we are starting in the right year
            // then we add number of weeks multiplied with days
            var result = firstThursday.AddDays(weekNum * 7);

            // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
            return result.AddDays(-3);
        }

        public static bool checkVideoFile(string extension)
        {
            bool res = false;
            IList<string> AllowedVideo = new List<string> { ".3gp", ".3g2", ".asf", ".avi", ".f4v", ".flv", ".ismv", ".m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".ogv", ".wmv", ".webm" };
            if (AllowedVideo.Contains(extension))
            {
                res = true;
            }
            return res;

        }

        public static bool checkImageFile(string extension)
        {
            bool res = false;
            IList<string> AllowedImage = new List<string> { ".bmp", ".exr", ".ico", ".jpg", ".jpeg", ".gif", ".pbm", ".pcx", ".pgm", ".png", ".ppm", ".psd", ".tif", ".tiff", ".tga", ".wbmp", ".heic" };
            if (AllowedImage.Contains(extension))
            {
                res = true;
            }
            return res;

        }

        public static DateTime ConvertStringToDate(string strDate)
        {
            DateTime dateTime = new DateTime(1890, 1, 1);
            try
            {
                dateTime = DateTime.FromOADate(Double.Parse(strDate));
            }
            catch
            {
                try
                {
                    //dateTime = DateTime.Parse(strDate);
                    dateTime = DateTime.ParseExact(strDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                    try
                    {
                        string[] strT = strDate.Split(' ');
                        if (strT.Length > 1)
                        {
                            string[] str1 = strT[0].Split('/');
                            string[] str2 = strT[1].Split(':');
                            if (str1.Length == 3)
                            {
                                dateTime = new DateTime(int.Parse(str1[2]), int.Parse(str1[1]), int.Parse(str1[0]), int.Parse(str2[0]), int.Parse(str2[1]), int.Parse(str2[2]));
                            }

                        }
                        else
                        {
                            string[] str = strDate.Split('/');
                            if (str.Length == 3)
                            {
                                dateTime = new DateTime(int.Parse(str[2]), int.Parse(str[1]), int.Parse(str[0]));
                            }
                        }
                    }
                    catch
                    {
                        string[] str = strDate.Split('/');
                        if (str.Length == 3)
                        {
                            dateTime = new DateTime(int.Parse(str[2]), int.Parse(str[1]), int.Parse(str[0]));
                        }
                    }
                }
            }


            return dateTime;
        }

        public static object TrimStringPropertyTypeObject(object obj)
        {
            var stringProperties = obj.GetType().GetProperties()
                          .Where(p => p.PropertyType == typeof(string));

            foreach (var stringProperty in stringProperties)
            {
                string currentValue = (string)stringProperty.GetValue(obj, null);
                if (currentValue != null)
                    stringProperty.SetValue(obj, currentValue.Trim(), null);
            }

            return obj;
        }

        public static string getCellValue(ICell cell)
        {
            //Lấy giá trị trong cell
            string str = "";
            try
            {
                if (cell.CellType.ToString().Equals("String"))
                    str = cell.StringCellValue;
                else if (cell.CellType.ToString().Equals("Numeric"))
                    str = cell.NumericCellValue + "";
                else if (cell.CellType.ToString().Equals("Formula"))
                {
                    try
                    {
                        str = cell.NumericCellValue + "";
                    }
                    catch
                    {
                        str = cell.StringCellValue + "";
                    }
                }
                else if (cell.CellType.ToString().Equals("Boolean"))
                    str = cell.BooleanCellValue + "";
                else if (cell.CellType.ToString().Equals("Date"))
                    str = cell.DateCellValue + "";
                else if (cell.CellType.ToString().Equals("Error"))
                    str = cell.ErrorCellValue + "";
                else if (cell.CellType.ToString().Equals("RichString"))
                    str = cell.RichStringCellValue + "";
            }
            catch { }
            return str.Trim();
        }

        public static string FormatMoney(decimal? s)
        {
            if (s == null) return "";

            string str = Convert.ToInt64(s).ToString();
            string stmp = str;
            int amount;
            amount = (int)(str.Length / 3);
            if (str.Length % 3 == 0)
                amount--;
            for (int i = 1; i <= amount; i++)
            {
                stmp = stmp.Insert(stmp.Length - 4 * i + 1, ".");
            }

            return stmp;
        }

        public static string ConvertMoneyToString(string money)
        {
            string[] dv = { "", "mươi", "trăm", "nghìn", "triệu", "tỉ" };
            string[] cs = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string doc;
            int i, j, k, n, len, found, ddv, rd;
            string[] arrListStr = money.Split('.');
            string number = arrListStr[0];
            len = number.Length;
            number += "ss";
            doc = "";
            found = 0;
            ddv = 0;
            rd = 0;

            i = 0;
            while (i < len)
            {
                //So chu so o hang dang duyet
                n = (len - i + 2) % 3 + 1;

                //Kiem tra so 0
                found = 0;
                for (j = 0; j < n; j++)
                {
                    if (number[i + j] != '0')
                    {
                        found = 1;
                        break;
                    }
                }

                //Duyet n chu so
                if (found == 1)
                {
                    rd = 1;
                    for (j = 0; j < n; j++)
                    {
                        ddv = 1;
                        switch (number[i + j])
                        {
                            case '0':
                                if (n - j == 3) doc += cs[0] + " ";
                                if (n - j == 2)
                                {
                                    if (number[i + j + 1] != '0') doc += "lẻ ";
                                    ddv = 0;
                                }
                                break;
                            case '1':
                                if (n - j == 3) doc += cs[1] + " ";
                                if (n - j == 2)
                                {
                                    doc += "mười ";
                                    ddv = 0;
                                }
                                if (n - j == 1)
                                {
                                    if (i + j == 0) k = 0;
                                    else k = i + j - 1;

                                    if (number[k] != '1' && number[k] != '0')
                                        doc += "mốt ";
                                    else
                                        doc += cs[1] + " ";
                                }
                                break;
                            case '5':
                                if (i + j == len - 1)
                                    doc += "lăm ";
                                else
                                    doc += cs[5] + " ";
                                break;
                            default:
                                doc += cs[(int)number[i + j] - 48] + " ";
                                break;
                        }

                        //Doc don vi nho
                        if (ddv == 1)
                        {
                            doc += dv[n - j - 1] + " ";
                        }
                    }
                }


                //Doc don vi lon
                if (len - i - n > 0)
                {
                    if ((len - i - n) % 9 == 0)
                    {
                        if (rd == 1)
                            for (k = 0; k < (len - i - n) / 9; k++)
                                doc += "tỉ ";
                        rd = 0;
                    }
                    else
                        if (found != 0) doc += dv[((len - i - n + 1) % 9) / 3 + 2] + " ";
                }

                i += n;
            }

            if (len == 1)
                if (number[0] == '0' || number[0] == '5') return cs[(int)number[0] - 48].ToUpper() + " đồng";

            doc = Regex.Replace(doc + " đồng", @"\s+", " ");
            return doc;
        }

        internal static DateTime getCellValue(DateTime dateTimeValue)
        {
            throw new NotImplementedException();
        }

        public static string NonUnicode(string text)
        {
            if (text == null) text = "";
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }

            text = Regex.Replace(text.ToLower().Replace(@"'", String.Empty), @"[^\w]+", "-").Replace("\"", "-").Replace(":", "-").ToLower().Trim();
            return Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
        }

        public static bool CheckPasswordValidParttern(string input)
        {
            return Regex.Match(input, partternPassword).Success;
        }

        public static string ConvertUrlpath(string text)
        {
            string res = "";
            if (text != null)
            {
                res = text;
                res = res.Replace("%2e%2e%2f", "../");
                res = res.Replace("%2e%2e", "../");
                res = res.Replace("..%2f", "../");
                res = res.Replace("%2e%2e%5c", "..\\");
                res = res.Replace("%2e%2e\\", "..\\");
                res = res.Replace("..%5c", "..\\");
                res = res.Replace("%252e%252e%255c", "..\\");
                res = res.Replace("..%255c", "..\\");

            }

            return res;
        }


        public static int DateDiff(DateTime FromDate, DateTime ToDate)
        {
            TimeSpan DateDiff = ToDate - FromDate;
            int nbDateDiff = DateDiff.Days;

            return nbDateDiff;
        }

        public static int MothDiff(DateTime FromDate, DateTime ToDate)
        {
            int MothDiff = (ToDate.Year - FromDate.Year) * 12 + ToDate.Month - FromDate.Month;
            return MothDiff;
        }

        public static int ExcessDay(DateTime FromDate, DateTime ToDate, int MonthDiff)
        {
            DateTime start = FromDate.AddMonths(MonthDiff);
            DateTime end = ToDate;



            // Lấy TimeSpan giữa hai ngày
            TimeSpan span = end.Subtract(start);

            int c = (int)span.TotalDays;

            if (c < 0)
            {
                c = 30 + c;
                MonthDiff--;
            }

            return c;

        }
    }
}
