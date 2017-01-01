using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Resistance.Helper
{
    public static class StringHelper
    {
        public static string ReEncode(this string beforeText)
        {
            string afterText = HttpUtility.UrlDecode(beforeText);
            return HttpUtility.HtmlEncode(afterText);
        }
    }
}