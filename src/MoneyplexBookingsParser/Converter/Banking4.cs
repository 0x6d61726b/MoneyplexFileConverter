/*--------------------------------------------------------------------------------------------------
Project:     OnlineBankingDataConverter
Description: Exporter/Importer for 'Banking4' (SUPA) format.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using Newtonsoft.Json;
using OnlineBankingDataConverter.Supa;
using System.Collections.Generic;
using System.IO;

namespace OnlineBankingDataConverter.Converter
{
    /// <summary>
    /// The class that can export or import content in 'Banking4' (SUPA) format.
    /// </summary>
    public static class Banking4
    {
        #region Exporter

        /// <summary>
        /// Exports the list of bookings to the given file in 'Banking4' (SUPA) JSON format.
        /// </summary>
        /// <param name="bookingList">The list of bookings to export.</param>
        /// <param name="file">The name of the export file.</param>
        /// <param name="batchBookingExport">
        /// The value that defines how to export bookings: <br /><c>null</c> to export all bookings;
        /// <br /><c>true</c> to export normal and collective bookings; <br /><c>false</c> to export
        /// partial bookings only.
        /// </param>
        public static void ExportJson(List<Booking> bookingList, string file,
            bool? batchBookingExport = null)
        {
            if (bookingList == null)
            {
                return;
            }

            // filter the bookings to export
            List<Booking> exportableBookings;
            if (batchBookingExport == null)
            {
                // export all bookings
                exportableBookings = bookingList;
            }
            else
            {
                exportableBookings = new List<Booking>();
                foreach (Booking booking in bookingList)
                {
                    if (batchBookingExport == true)
                    {
                        // export normal and collective bookings
                        if ((booking.BatchBooking == null) || (booking.BatchBooking == true))
                        {
                            exportableBookings.Add(booking);
                        }
                    }
                    else
                    {
                        // export only partial bookings
                        if (booking.BatchBooking == false)
                        {
                            exportableBookings.Add(booking);
                        }
                    }
                }
            }

            // convert data to JSON string
            string jsonStr = JsonConvert.SerializeObject(exportableBookings, Formatting.Indented,
            Booking.JsonSerializerSettings);

            // write JSON string to file
            File.WriteAllText(file, jsonStr);
        }

        #endregion

        #region Importer

        /// <summary>
        /// Imports a JSON file exported by Banking4 that contains bookings.
        /// </summary>
        /// <param name="file">The JSON file exported by Banking4.</param>
        /// <returns>The imported <see cref="List{Booking}" />.</returns>
        public static List<Booking> ImportJson(string file)
        {
            // read JSON string from file
            string jsonStr = File.ReadAllText(file);

            // convert JSON string to list of bookings
            List<Booking> bookings = JsonConvert.DeserializeObject<List<Booking>>(jsonStr);

            return bookings;
        }

        #endregion
    }
}