/*--------------------------------------------------------------------------------------------------
Project:     MoneyplexFileConverter
Description: Post-processor related to 'Commerzbank' bookings.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using OnlineBankingDataConverter.Supa;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MoneyplexFileConverter.PostProcessor
{
    /// <summary>
    /// The post-processor class related to 'Commerzbank' bookings.
    /// </summary>
    internal static class Commerzbank
    {
        /// <summary>
        /// Runs post-processing for the list of bookings from 'Commerzbank'.
        /// </summary>
        /// <param name="bookings">The bookings to process.</param>
        internal static void Process(ref List<Booking> bookings)
        {
            if (bookings == null)
            {
                return;
            }

            // process all bookings
            foreach (Booking booking in bookings)
            {
                string purpose = booking.RemittanceInformation;

                // check if a purpose content is available
                if (purpose == null)
                {
                    continue;
                }

                // determine used delimiter
                string delimiter = Utils.GetDelimiter(purpose);

                // try to extract SEPA booking text (located in the last field)
                Match sepaMatch = Regex.Match(purpose,
                    $@"(?:^|{Regex.Escape(delimiter)})(?'sepa'SEPA-[A-Z]{2}\ .+?)$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (sepaMatch.Success)
                {
                    // assign SEPA booking text
                    booking.BookingText = sepaMatch.Groups["sepa"].Value.Trim();

                    // remove SEPA booking text from XML purpose element content
                    purpose = purpose.Substring(0, sepaMatch.Index);
                }
                else
                {
                    // when no SEPA booking text was found, the non-SEPA booking text is expected in
                    // the last field of the XML purpose text
                    if (purpose.Contains(delimiter))
                    {
                        int lastIndex = purpose.LastIndexOf(delimiter);
                        booking.BookingText = purpose.Substring(lastIndex + delimiter.Length).Trim();

                        // remove booking text from XML purpose element content
                        purpose = purpose.Substring(0, lastIndex);
                    }
                }

                // process key/value pairs of purpose
                Utils.ProcessPurposeKeyValuePairs(booking, purpose, delimiter);
            }
        }
    }
}