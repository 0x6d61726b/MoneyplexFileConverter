/*--------------------------------------------------------------------------------------------------
Project:     OnlineBankingDataConverter
Description: Extension methods for 'System.string'.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using System;
using System.Globalization;

namespace OnlineBankingDataConverter.Utils
{
    /// <summary>
    /// The class that provides extension methods for 'System.string' (specific to this assembly).
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// The German culture information.
        /// </summary>
        internal static readonly CultureInfo GermanCulture = new CultureInfo("de-DE");

        /// <summary>
        /// Converts the left-hand German culture string to <see cref="DateTime" />.
        /// </summary>
        /// <param name="str">The date string in German culture.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        internal static DateTime ToDateTime(this string str)
        {
            return DateTime.ParseExact(str, "dd.MM.yy", GermanCulture);
        }

        /// <summary>
        /// Converts the left-hand German culture string to <see cref="decimal" />.
        /// </summary>
        /// <param name="str">The decimal string in German culture.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        internal static decimal ToDecimal(this string str)
        {
            return decimal.Parse(str, GermanCulture);
        }
    }
}