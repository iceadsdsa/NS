using System;
using System.Globalization;

namespace NS
{
    internal static class L
    {
        public static bool En = Detect();

        static bool Detect()
        {
            try
            {
                string two = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                return !two.Equals("zh", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        public static string T(string zh, string en) { return En ? en : zh; }

        public static string Cat(string language)
        {
            return En && language == "开发工具" ? "Tools" : language;
        }
    }
}
