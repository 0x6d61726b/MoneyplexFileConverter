/*--------------------------------------------------------------------------------------------------
Project:     MoneyplexFileConverter
Description: Post-processor related to 'PSD Bank' and 'DKB' bookings.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using OnlineBankingDataConverter.Supa;
using System.Collections.Generic;

namespace MoneyplexFileConverter.PostProcessor
{
    /// <summary>
    /// The post-processor class related to 'PSD Bank' and 'DKB' bookings.
    /// </summary>
    internal static class PsdBank
    {
        /// <summary>
        /// Runs post-processing for the list of bookings from 'PSD Bank'.
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

                // extract SEPA booking text (located in last field)
                if (purpose.Contains(delimiter))
                {
                    int lastIndex = purpose.LastIndexOf(delimiter);
                    booking.BookingText = purpose.Substring(lastIndex + delimiter.Length).Trim();

                    // remove booking text from purpose field
                    purpose = purpose.Substring(0, lastIndex);
                }

                // cleanup aligned '@' delimiter
                if (purpose.Contains("@") == true)
                {
                    purpose = purpose.RemoveAlignedDelimiter("@", 27);
                }

                // process key/value pairs of purpose
                Utils.ProcessPurposeKeyValuePairs(booking, purpose, delimiter);
            }
        }
    }
}