using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client;

public static class StringExt
{
    public static string SFormat(this string str, params object[] args)
    {
        return string.Format(str, args);
    }

    public static string Truncate(this string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || maxLength < 1)
        {
            return string.Empty;
        }

        if (input.Length <= maxLength)
        {
            return input;
        }

        return input.Substring(0, maxLength - 3) + "...";
    }
    
}
