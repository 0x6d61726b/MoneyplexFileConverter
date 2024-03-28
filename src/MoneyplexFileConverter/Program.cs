﻿/*--------------------------------------------------------------------------------------------------
Project:     MoneyplexFileConverter
Description: The demo application that imports moneyplex XML exported files and export to Banking4
             format.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using MoneyplexFileConverter.PostProcessor;
using OnlineBankingDataConverter.Converter;
using OnlineBankingDataConverter.Importer;
using OnlineBankingDataConverter.Supa;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MoneyplexFileConverter
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // set invariant (English) language
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            // import test account with bookings
            List<Account> testAccount =
                Moneyplex.ImportXml(@"..\..\test-data\MoneyplexXmlExport.xml") as List<Account>;
            List<Booking> testBookings = testAccount.First().Bookings;

            // post-process moneyplex (raw) purpose field
            Commerzbank.Process(ref testBookings);

            // write bookings content to text file (for debugging purpose)
            testBookings.PrintBookings(@"..\..\test-data\MoneyplexXmlExport.print.txt");

            // print all moneyplex categories (used for completion of mapping method)
            testBookings.PrintCategories(@"..\..\test-data\MoneyplexXmlExport.categories.txt");

            // export all bookings to be imported by banking4
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            // IMPORTANT: Banking4 v8.4 DOES NOT support import of split bookings. Therefore, the
            // list of bookings is split into a list of individual bookings and a list of collective
            // bookings that can be imported individually. For importing the split bookings, an
            // AutoHotkey script was written and the required data are generated by this program.
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
#if false
            // export combined import file (contains individual/collective/split bookings)
            Banking4.ExportJson(testBookings, @"..\..\test-data\MoneyplexXmlExport.banking4.json");
#else
            // export separate import files for individual, collective and split bookings
            List<Booking> testBookingsIndividual = new List<Booking>();
            List<Booking> testBookingsCollective = new List<Booking>();
            List<Booking> testBookingsSplit = new List<Booking>();
            Dictionary<string, List<Booking>> batchBookingList = new Dictionary<string, List<Booking>>();
            foreach (Booking booking in testBookings)
            {
                if (booking.BatchBooking == false)
                {
                    // split booking found
                    testBookingsSplit.Add(booking);

                    if (batchBookingList.ContainsKey(booking.BatchId) == false)
                    {
                        batchBookingList.Add(booking.BatchId, new List<Booking>());
                    }

                    batchBookingList[booking.BatchId].Add(booking);
                }
                else if (booking.BatchBooking == true)
                {
                    // collective booking found
                    testBookingsCollective.Add(booking);
                }
                else
                {
                    // individual booking found
                    testBookingsIndividual.Add(booking);
                }
            }

            // export individual and collective bookings to file
            Banking4.ExportJson(testBookingsIndividual,
                @"..\..\test-data\MoneyplexXmlExport.individual.json");
            Banking4.ExportJson(testBookingsCollective,
                @"..\..\test-data\MoneyplexXmlExport.collective.json");

            // export AutoHotkey script data
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            StringBuilder autoHotkeyStringBuilder = new StringBuilder();
            foreach (var bookingList in batchBookingList)
            {
                // check if list is empty
                List<Booking> list = bookingList.Value;
                if (list.Count == 0)
                {
                    continue;
                }

                // calculate total amount
                decimal totalAmount = (decimal)0.00;
                foreach (Booking booking in list)
                {
                    totalAmount += (decimal)booking.AmountSigned;
                }

                autoHotkeyStringBuilder
                    .Append($"\tif ((booking[\"Name\"] = \"{list.First().RemittedName}\") and ")
                    .Append($"(booking[\"Info\"] = \"{list.First().RemittanceInformation}\") and ")
                    .Append($"(booking[\"Amount\"] = \"{Math.Abs(totalAmount):N2}\"))")
                    .AppendLine()
                    .AppendLine("\t{")
                    .AppendLine("\t\tsplitBooking := Array()");
                foreach (Booking booking in list)
                {
                    string creditDebitStr;
                    switch (booking.CreditDebitIndicator)
                    {
                        case CreditDebit.Credit:
                            creditDebitStr = "h";
                            break;

                        case CreditDebit.Debit:
                            creditDebitStr = "s";
                            break;

                        default:
                            creditDebitStr = string.Empty;
                            break;
                    }

                    autoHotkeyStringBuilder
                        .Append("\t\tsplitBooking.Push(Map(")
                        .Append($"\"Category\", \"{booking.Category}\", ")
                        .Append($"\"Amount\", \"{((decimal)booking.Amount):N2}\", ")
                        .Append($"\"CreditDebit\", \"{creditDebitStr}\"))")
                        .AppendLine();
                }
                autoHotkeyStringBuilder
                    .AppendLine("\t\treturn splitBooking")
                    .AppendLine("\t}");
            }
            File.WriteAllText(@"..\..\test-data\MoneyplexXmlExport.split.ahk.txt",
                autoHotkeyStringBuilder.ToString());
#endif
        }

        /// <summary>
        /// Prints the list of bookings to the provided text file path (for debugging).
        /// </summary>
        /// <param name="bookings">The list of bookings.</param>
        /// <param name="path">The path to the output text file.</param>
        private static void PrintBookings(this List<Booking> bookings, string path)
        {
            int keyMaxLen = typeof(Booking).GetProperties().OrderByDescending(s => s.Name.Length)
                .First().Name.Length;

            // create report
            StringBuilder report = new StringBuilder(string.Empty);
            foreach (Booking booking in bookings)
            {
                // report all booking property fields
                foreach (PropertyInfo prop in booking.GetType().GetProperties().OrderBy(n => n.Name))
                {
                    string value = prop.GetValue(booking)?.ToString();
                    if ((value != null) || (prop.Name == "RemittanceInformation"))
                    {
                        if (value == null)
                        {
                            value = "<null>";
                        }
                        report.AppendLine((prop.Name + ": ").PadRight(keyMaxLen + 2) + value);
                    }
                }
                report.AppendLine();
            }

            // adjust file extension
            if (!Path.GetExtension(path).Equals(".TXT", StringComparison.InvariantCultureIgnoreCase))
            {
                path += ".txt";
            }

            // save report
            File.WriteAllText(path, report.ToString());
        }

        /// <summary>
        /// Prints the list of categories to the provided text file path.
        /// </summary>
        /// <param name="bookings">The list of bookings.</param>
        /// <param name="path">The path to the output text file.</param>
        private static void PrintCategories(this List<Booking> bookings, string path)
        {
            // get all categories from the list of bookings
            List<string> categoryList = new List<string>();
            foreach (Booking booking in bookings)
            {
                if ((string.IsNullOrEmpty(booking.Category) == false) &&
                    (categoryList.Contains(booking.Category) == false))
                {
                    categoryList.Add(booking.Category);
                }
            }

            // adjust file extension
            if (!Path.GetExtension(path).Equals(".TXT", StringComparison.InvariantCultureIgnoreCase))
            {
                path += ".txt";
            }

            // save categories to file
            File.WriteAllText(path, string.Join(Environment.NewLine, categoryList.OrderBy(x => x)));
        }
    }
}