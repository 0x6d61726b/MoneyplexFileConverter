/*--------------------------------------------------------------------------------------------------
Project:     MoneyplexFileConverter
Description: Post-processor related to 'Sparda-Bank BW' bookings.
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
    /// The post-processor class related to 'Sparda-Bank BW' bookings.
    /// </summary>
    internal static class SpardaBankBw
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
            bool hadSepaMatch = false;
            foreach (Booking booking in bookings)
            {
                string purpose = booking.RemittanceInformation;

                // check if a purpose content is available
                if (purpose != null)
                {
                    // determine used delimiter
                    string delimiter = Utils.GetDelimiter(purpose);

                    // try to extract SEPA booking text (located in first field)
                    Match sepaMatch = Regex.Match(purpose,
                        $@"^(?'sepa'SEPA[\ \-].+?)(?:{Regex.Escape(delimiter)}|$)",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (sepaMatch.Success)
                    {
                        // set flag that a SEPA match was found
                        hadSepaMatch = true;

                        // assign SEPA booking text
                        booking.BookingText = sepaMatch.Groups["sepa"].Value.Trim();

                        // remove SEPA booking text from XML purpose element content
                        purpose = purpose.Substring(sepaMatch.Index + sepaMatch.Length);
                    }

                    // cleanup aligned '@' delimiter
                    if (purpose.Contains(delimiter) == true)
                    {
                        purpose = purpose.RemoveAlignedDelimiter(delimiter, 27);
                    }

                    // extract name (if there was not yet a SEPA entry)
                    if ((hadSepaMatch == false) && (delimiter == "@") && purpose.Contains(delimiter))
                    {
                        int index = purpose.IndexOf(delimiter);
                        booking.RemittedName = purpose.Substring(0, index).ToNull();
                        purpose = purpose.Substring(index + delimiter.Length);
                    }

                    // process key/value pairs of purpose
                    Utils.ProcessPurposeKeyValuePairs(booking, purpose, delimiter);
                }

                // fix RemittedName/RemittanceInformation
                if (booking.RemittanceInformation == null)
                {
                    switch (booking.RemittedName.ToUpperInvariant())
                    {
                        case "GUTSCHRIFT":
                        case "KREDITAUSZAHL.":
                        case "KREDITZINSEN":
                        case "KREDIT-RATE":
                        case "RECHNUNGSABSCHL":
                            booking.RemittanceInformation = booking.RemittedName;
                            booking.RemittedName = null;
                            break;
                    }
                }
            }
        }
    }
}