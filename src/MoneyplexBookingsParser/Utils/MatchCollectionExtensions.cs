/*--------------------------------------------------------------------------------------------------
Project:     OnlineBankingDataConverter
Description: Extension methods for 'System.Text.RegularExpressions.MatchCollection'.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OnlineBankingDataConverter.Utils
{
    /// <summary>
    /// The class providing extension methods for 'System.Text.RegularExpressions.MatchCollection'.
    /// </summary>
    internal static class MatchCollectionExtensions
    {
        /// <summary>
        /// Determines whether the specified <see cref="MatchCollection" /> contains the specified
        /// value.
        /// </summary>
        /// <param name="matchCollection">The match collection in which to locate a value.</param>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <returns>
        /// <c>true</c> if the specified value contains value; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ContainsValue(this MatchCollection matchCollection, string value)
        {
            return matchCollection.Cast<Match>().Select(x => x.Value).Contains(value);
        }
    }
}