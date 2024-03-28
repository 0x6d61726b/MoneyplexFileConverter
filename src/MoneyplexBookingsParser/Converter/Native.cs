/*--------------------------------------------------------------------------------------------------
Project:     OnlineBankingDataConverter
Description: Exporter/Importer for native .NET XML format.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using OnlineBankingDataConverter.Supa;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace OnlineBankingDataConverter.Converter
{
    /// <summary>
    /// The class that can export imported content in <see cref="OnlineBankingDataConverter" />
    /// native format.
    /// </summary>
    public static class Native
    {
        #region Exporter

        /// <summary>
        /// Exports the list of bookings to the given file as native .NET XML file.
        /// </summary>
        /// <param name="data">The list of bookings to export.</param>
        /// <param name="file">The name of the export file.</param>
        public static void ExportXml(this List<Booking> data, string file)
        {
            if (data == null)
            {
                return;
            }

            // create serializer
            XmlSerializer serializer = new XmlSerializer(data.GetType());

            // export data
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { Indent = true };
            using (XmlWriter xmlWriter = XmlWriter.Create(file, xmlWriterSettings))
            {
                serializer.Serialize(xmlWriter, data);
            }
        }

        #endregion

        #region Importer

        /// <summary>
        /// Imports a native .NET XML file exported by this class that contains bookings.
        /// </summary>
        /// <param name="file">The native .NET XML file exported by this class.</param>
        /// <returns>The imported <see cref="List{Booking}" />.</returns>
        public static List<Booking> ImportXml(string file)
        {
            // create serializer
            XmlSerializer serializer = new XmlSerializer(typeof(List<Booking>));

            // import data
            using (XmlReader xmlReader = XmlReader.Create(file))
            {
                List<Booking> bookings = serializer.Deserialize(xmlReader) as List<Booking>;

                return bookings;
            }
        }

        #endregion
    }
}