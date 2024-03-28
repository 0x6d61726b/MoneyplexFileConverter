/*--------------------------------------------------------------------------------------------------
Project:     OnlineBankingDataConverter
Description: Importer for files exported by moneyplex online banking software.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using OnlineBankingDataConverter.Supa;
using OnlineBankingDataConverter.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OnlineBankingDataConverter.Importer
{
    /// <summary>
    /// The class that can import files exported by moneyplex online banking software.
    /// </summary>
    public static class Moneyplex
    {
        /// <summary>
        /// Imports an XML file exported by moneyplex that contains accounts and/or bookings.
        /// </summary>
        /// <param name="file">The XML file exported by moneyplex.</param>
        /// <returns>
        /// The imported <see cref="List{Account}" /> or <see cref="List{Booking}" />.
        /// </returns>
        public static object ImportXml(string file)
        {
            // load the XML file
            using (var fileStream = new StreamReader(file, Encoding.GetEncoding(1252)))
            {
                XDocument xmlDoc = XDocument.Load(fileStream);

                // check if file contains account(s)
                switch (xmlDoc.Root.Name.ToString())
                {
                    case "KONTO":
                        // XML file contains accounts (and bookings)
                        return ImportAccounts(xmlDoc);

                    case "KONTOBUCH":
                        // XML file contains bookings
                        return ImportBookings(xmlDoc);

                    default:
                        // XML file contains unsupported root element
                        throw new InvalidDataException($"Unsupported moneyplex XML file format " +
                            $"(XML root element: '{xmlDoc.Root.Name}').");
                }
            }
        }

        /// <summary>
        /// Imports the accounts contained in the XML file exported by moneyplex.
        /// </summary>
        /// <param name="xmlDoc">The XML file exported by moneyplex.</param>
        /// <returns>The imported <see cref="List{Account}" />.</returns>
        private static List<Account> ImportAccounts(XDocument xmlDoc)
        {
            List<Account> accountList = new List<Account>();

            // import all accounts
            foreach (XElement xml in xmlDoc.Elements("KONTO"))
            {
                Account account = new Account
                {
                    AccountBankCode = xml.Element("BLZ")?.Value,
                    AccountBIC = xml.Element("BIC")?.Value,
                    AccountCurrency = xml.Element("WAEHRUNG")?.Value,
                    AccountIBAN = xml.Element("IBAN")?.Value,
                    AccountName = xml.Element("BEZEICHNUNG")?.Value,
                    AccountNumber = xml.Element("KONTONR")?.Value,
                    AccountTypeCode = xml.Element("KONTOART")?.Value.ToAccountTypeCode(),
                };

                // add bookings to account
                if (xml.Element("KONTOBUCH")?.IsEmpty == false)
                {
                    account.Bookings =
                        ImportBookings(new XDocument(xml.Element("KONTOBUCH")), account);
                }

                // done, add the processed account to the list of accounts
                accountList.Add(account);
            }

            return (accountList.Count > 0 ? accountList : null);
        }

        /// <summary>
        /// Imports the account bookings contained in the XML file exported by moneyplex.
        /// </summary>
        /// <param name="xmlDoc">The XML file exported by moneyplex.</param>
        /// <param name="account">The account the bookings belong to.</param>
        /// <returns>The imported <see cref="List{Booking}" />.</returns>
        private static List<Booking> ImportBookings(XDocument xmlDoc, Account account = null)
        {
            List<Booking> bookingList = new List<Booking>();

            // import all bookings
            foreach (XElement xml in xmlDoc.Element("KONTOBUCH").Elements("BUCHUNG"))
            {
                Booking booking = new Booking(account)
                {
                    AmountSigned = xml.Element("BETRAG")?.Value.ToDecimal(),
                    AmountCurrency = xml.Element("WAEHRUNG")?.Value,
                    BookingDate = xml.Element("DATUM")?.Value.ToDateTime(),
                    Category = xml.Element("KATEGORIE")?.Value,
                    RemittanceInformation = xml.Element("ZWECK")?.Value,
                    RemittedAccountBankCode = xml.Element("EMPFAENGER")?.Element("BLZ")?.Value,
                    RemittedAccountBIC = xml.Element("EMPFAENGER")?.Element("BIC")?.Value,
                    RemittedAccountIBAN = xml.Element("EMPFAENGER")?.Element("IBAN")?.Value,
                    RemittedAccountNumber = xml.Element("EMPFAENGER")?.Element("KONTONR")?.Value,
                    RemittedName = GetRemittedName(
                        xml.Element("EMPFAENGER")?.Element("NAME")?.Value,
                        xml.Element("EMPFAENGER")?.Element("ZUSATZ")?.Value),
                    ValueDate = xml.Element("VALUTA")?.Value.ToDateTime(),
                };

                // assign an Internal ID
                booking.AssignInternalId(ref bookingList);

                // check if booking has split bookings
                bool hasSplitBookings = (xml.Element("SPLITT")?.HasElements == true);
                if (hasSplitBookings)
                {
                    // mark this booking as collective booking
                    booking.BatchBooking = true;
                }

                // done, add the processed booking to the list of bookings
                bookingList.Add(booking);

                // process split bookings
                if (hasSplitBookings)
                {
                    // process all split bookings
                    foreach (XElement part in xml.Element("SPLITT").Elements("PART"))
                    {
                        // clone the collective booking
                        Booking splitBooking = booking.Clone();

                        // mark this booking as split booking
                        splitBooking.BatchBooking = false;
                        splitBooking.BatchId = booking.Id;

                        // add split booking properties
                        splitBooking.AmountSigned = part.Element("BETRAG")?.Value.ToDecimal();
                        splitBooking.Category = part.Element("KATEGORIE")?.Value;
                        splitBooking.Notes = part.Element("ZWECK")?.Value;

                        // assign an Internal ID
                        splitBooking.AssignInternalId(ref bookingList);

                        // done, add the processed split booking to the list of bookings
                        bookingList.Add(splitBooking);
                    }
                }
            }

            return (bookingList.Count > 0 ? bookingList : null);
        }

        #region Helper methods

        /// <summary>
        /// Assigns the internal identifier for the booking.
        /// </summary>
        /// <param name="booking">The booking.</param>
        /// <param name="bookingList">The list of bookings.</param>
        private static void AssignInternalId(this Booking booking, ref List<Booking> bookingList)
        {
            for (int count = 0; true; count++)
            {
                booking.Id = booking.CalculateHashId(count);
                if (bookingList.Where(x => x.Id == booking.Id).Any() == false)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Returns the assembled name of the remitter.
        /// </summary>
        /// <param name="name">The name part.</param>
        /// <param name="nameAddition">The name annex part.</param>
        /// <returns>The assembled name of the remitter.</returns>
        private static string GetRemittedName(string name, string nameAddition)
        {
            if (string.IsNullOrEmpty(nameAddition))
            {
                return name;
            }
            else if (name.Length == 27)
            {
                // concatenate both strings (without white-space)
                return name + nameAddition;
            }
            else
            {
                // concatenate both strings with white-space in between
                return name + " " + nameAddition;
            }
        }

        /// <summary>
        /// Converts the provided account type string to an <see cref="AccountTypeCode" />.
        /// </summary>
        /// <param name="accountType">The provided account type.</param>
        /// <returns>The <see cref="AccountTypeCode" /> from provided account type string.</returns>
        private static AccountTypeCode ToAccountTypeCode(this string accountType)
        {
            switch (accountType.ToUpperInvariant())
            {
                case "BARGELDKONTO":
                case "BAUSPARKONTO":
                case "SPARBUCH":
                    return AccountTypeCode.Cash;

                case "GIROKONTO":
                    return AccountTypeCode.Checking;

                case "KREDITKARTENKONTO":
                    return AccountTypeCode.CreditCard;

                case "FESTGELDKONTO":
                    return AccountTypeCode.FixedTermDeposit;

                default:
                    throw new NotImplementedException(
                        $"AccountTypeCode mapping for '{accountType}' not implemented.");
            }
        }

        #endregion
    }
}