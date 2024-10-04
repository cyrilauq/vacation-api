using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Utils
{
    internal static class DateTimeUtils
    {
        public static String DateFormat(this DateTime dateTime) => dateTime.ToString("dd/MM/yyyy");
        public static String TimeFormat(this DateTime dateTime) => dateTime.ToString("HH:mm");
    }
}
