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

    /// <summary>
    /// Indicates whether the specified string is null or an empty string ("").
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <returns><see langword="true"/> if the <see href="input"/> parameter is <see langword="null"/> 
    ///     or an empty string (<see cref="''"/>); otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsEmpty(this string input)
    {
        return string.IsNullOrEmpty(input);
    }

    /// <summary>
    /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <returns><see langword="true"/> if the <see href="input"/> parameter is <see langword="null"/> 
    ///     or <see cref="String.Empty"/>, or if value consists exclusively of white-space characters.
    /// </returns>
    public static bool IsNullOrSpace(this string input)
    {
        return string.IsNullOrWhiteSpace(input);
    }
}
