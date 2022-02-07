using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InverseCurveSidebarBot
{
    public class ShortHandCurrencyFormatter : ICustomFormatter, IFormatProvider
    {
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            if (arg == null)
            {
                return "?";
            }

            if (format == null || !format.Trim().StartsWith("SH"))
            {
                if (arg is IFormattable)
                {
                    return ((IFormattable)arg).ToString(format, formatProvider);
                }

                return arg.ToString();
            }

            if (arg is int num)
            {
                var figures = (int)Math.Floor(Math.Log10(num) + 1);
                var prefix = NumberFormatInfo.CurrentInfo.CurrencySymbol;
                string suffix = string.Empty;

                switch (figures)
                {
                    case >= 4 and < 7:
                        num /= 1000;
                        suffix = "k";
                        break;
                    case >= 7 and < 10:
                        num /= 1000000;
                        suffix = "b";
                        break;
                }

                return $"{prefix}{num}{suffix}";
            }

            return arg.ToString();
        }

        public object? GetFormat(Type? formatType)
        {
            return (formatType == typeof(ICustomFormatter)) ? this : null;
        }
    }
}
