using OnlineBankingDataConverter.Supa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MoneyplexFileConverter.PostProcessor
{
    /// <summary>
    /// The class that contains utility methods for post-processing.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Gets the delimiter used by moneyplex.
        /// </summary>
        /// <param name="str">The raw purpose string exported by moneyplex.</param>
        /// <returns>The delimiter used by moneyplex.</returns>
        internal static string GetDelimiter(string str)
        {
            if (str == null)
            {
                return null;
            }

            // check for delimiter (moneyplex uses either '@' or " ")
            string delimiter = "  ";
            if (str.Contains('@'))
            {
                delimiter = "@";
            }

            return delimiter;
        }

        /// <summary>
        /// Maps the category field provided by moneyplex to the category field valid in Banking4.
        /// </summary>
        /// <param name="bookings">The list of bookings.</param>
        internal static void MapCategoryMoneyplexToBanking4(this List<Booking> bookings)
        {
            foreach (Booking booking in bookings)
            {
                booking.MapCategoryMoneyplexToBanking4();
            }
        }

        /// <summary>
        /// Maps the category field provided by moneyplex to the category field valid in Banking4.
        /// </summary>
        /// <param name="booking">The booking.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static void MapCategoryMoneyplexToBanking4(this Booking booking)
        {
            // map categories
            switch (booking.Category)
            {
                case null:
                    break;

                case "[Umbuchung]":
                    booking.Category = "(Umbuchung)";
                    break;

                // TODO: extend to your needs ...

                default:
                    throw new NotImplementedException(
                        $"Category mapping for '{booking.Category}' not implemented");
            }
        }

        /// <summary>
        /// Processes the key value pairs found in the purpose string.
        /// </summary>
        /// <param name="booking">The booking.</param>
        /// <param name="purpose">The purpose string.</param>
        /// <param name="delimiter">The used delimiter.</param>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static void ProcessPurposeKeyValuePairs(this Booking booking, string purpose,
            string delimiter)
        {
            // check if a purpose content is available
            if (purpose == null)
            {
                return;
            }

            // clear remittanceInformation
            booking.RemittanceInformation = null;

            // search for purpose key/value pairs
            MatchCollection matches = Regex.Matches(purpose, @"(?:^|[ @])" +
                @"(?'key'(IBAN[:+])|(BIC[:+])|(EREF[:+])|(KREF[:+])|(MREF[:+])|(CRED[:+])|" +
                @"(SVWZ[:+])|(EndtoEnd:)|(Kundenref\.:)|(Mandatsref\.:)|(Creditor\-ID:)|" +
                @"(Debitor\-ID:)|(KTO/BLZ)|(Purpose:))[ ]*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                // process all found keys
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    // extract the key
                    string key = matches[i].Groups["key"].Value.ToUpperInvariant()
                        .Replace("+", "").Replace(":", "").Trim();

                    // extract the value
                    string value;
                    if (i < (matches.Count - 1))
                    {
                        // extract first or in-between value of key/value pair
                        value = purpose.Substring(matches[i].Index + matches[i].Length,
                            matches[i + 1].Index - (matches[i].Index + matches[i].Length));
                    }
                    else
                    {
                        // extract last value of key/value pair
                        value = purpose.Substring(matches[i].Index + matches[i].Length);
                    }

                    // moneyplex uses a non-aligned delimiter to separate data
                    int splitPos = value.FindFirstNonAlignedDelimiter(22, delimiter);
                    if ((splitPos >= 0) && (splitPos < value.Length))
                    {
                        value = value.Substring(0, splitPos);
                    }
                    int valueRawLength = value.Length;

                    // remove key/value pair from purpose field
                    purpose = purpose.Substring(0, matches[i].Index) +
                    purpose.Substring(matches[i].Index + matches[i].Length + valueRawLength);

                    // update value (remove delimiters inserted by moneyplex)
                    value = value.RemoveAlignedDelimiter(delimiter, 22)
                        .ReplaceRecursive(delimiter, " ").Trim('@', ' ');

                    // process key to assign value to booking
                    switch (key)
                    {
                        case "IBAN":
                            if (string.IsNullOrEmpty(booking.RemittedAccountIBAN))
                            {
                                booking.RemittedAccountIBAN = value;
                            }
                            else if (booking.RemittedAccountIBAN != value)
                            {
                                throw new InvalidDataException($"Booking already contains a " +
                                    $"value for '{nameof(Booking.RemittedAccountIBAN)}'.");
                            }
                            break;

                        case "BIC":
                            if (string.IsNullOrEmpty(booking.RemittedAccountBIC))
                            {
                                booking.RemittedAccountBIC = value;
                            }
                            else if (booking.RemittedAccountBIC != value)
                            {
                                throw new InvalidDataException($"Booking already contains a " +
                                    $"value for '{nameof(Booking.RemittedAccountBIC)}'.");
                            }
                            break;

                        case "EREF":
                        case "ENDTOEND":
                            if (string.IsNullOrEmpty(booking.EndToEndId))
                            {
                                booking.EndToEndId = value;
                            }
                            else if (booking.EndToEndId != value)
                            {
                                throw new InvalidDataException($"Booking already contains a " +
                                    $"value for '{nameof(Booking.EndToEndId)}'.");
                            }
                            break;

                        case "KREF":
                        case "KUNDENREF.":
                            if (string.IsNullOrEmpty(booking.PaymentInformationId))
                            {
                                booking.PaymentInformationId = value;
                            }
                            else if (booking.PaymentInformationId != value)
                            {
                                throw new InvalidDataException($"Booking already contains a " +
                                    $"value for '{nameof(Booking.PaymentInformationId)}'.");
                            }
                            break;

                        case "MREF":
                        case "MANDATSREF.":
                            if (string.IsNullOrEmpty(booking.MandateId))
                            {
                                booking.MandateId = value;
                            }
                            else if (booking.MandateId != value)
                            {
                                throw new InvalidDataException($"Booking already contains a " +
                                    $"value for '{nameof(Booking.MandateId)}'.");
                            }
                            break;

                        case "CRED":
                        case "CREDITOR-ID":
                        case "DEBITOR-ID":
                            if (string.IsNullOrEmpty(booking.CreditorId))
                            {
                                booking.CreditorId = value;
                            }
                            else if (booking.CreditorId != value)
                            {
                                throw new InvalidDataException($"Booking already contains a " +
                                    $"value for '{nameof(Booking.CreditorId)}'.");
                            }
                            break;

                        case "SVWZ":
                            booking.RemittanceInformation =
                                value.ReplaceRecursive("  ", " ").Trim().ToNull();
                            break;

                        case "KTO/BLZ":
                            string[] valueSplit = value.Split('/');
                            booking.RemittedAccountNumber = valueSplit[0].TrimStart('0');
                            booking.RemittedAccountBankCode = valueSplit[1];
                            break;

                        case "PURPOSE":
                            // ignored for now
                            break;

                        default:
                            throw new NotImplementedException(
                                $"Key '{key}' found in purpose field not implemented");
                    }
                }
            }

            // cleanup delimiters and trim remaining purpose field
            purpose = purpose.Replace("@", " ").ReplaceRecursive("  ", " ").ToNull();

            // assign remaining purpose to booking
            if (purpose != null)
            {
                booking.RemittanceInformation += purpose;
            }
            booking.RemittanceInformation = booking.RemittanceInformation?.Trim().ToNull();
        }

        /// <summary>
        /// Removes the delimiter in the provided string that is located at aligned positions.
        /// </summary>
        /// <param name="str">The string the delimiter shall be replaced.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="alignedPos">
        /// The position (and multiple of it) at which the delimiter shall be replaced.
        /// </param>
        /// <returns>The provided string with delimiters removed at aligned positions.</returns>
        internal static string RemoveAlignedDelimiter(this string str, string delimiter,
            int alignedPos)
        {
            if (str == null)
            {
                return null;
            }

            // find all aligned delimiters
            int indexPos, startIndex = 0;
            StringBuilder sb = new StringBuilder();
            while ((indexPos = str.IndexOf(delimiter, startIndex)) >= 0)
            {
                sb.Append(str.Substring(startIndex, indexPos - startIndex));
                if ((indexPos - startIndex) != alignedPos)
                {
                    sb.Append(delimiter);
                }

                startIndex = indexPos + delimiter.Length;
            }
            if (startIndex < str.Length)
            {
                sb.Append(str.Substring(startIndex));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts the provided string to null if it is empty.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>
        /// <c>null</c> if the provided string is empty; otherwise, the provided string.
        /// </returns>
        internal static string ToNull(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// Finds the position of the first non-aligned delimiter.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="alignedPos">The first position an aligned delimiter is expected.</param>
        /// <param name="delimiter">The delimiter to be searched.</param>
        /// <returns>The position of the first non-aligned delimiter; otherwise, <c>-1</c>.</returns>
        private static int FindFirstNonAlignedDelimiter(this string str, int alignedPos,
            string delimiter)
        {
            int indexPos, startIndex = 0;
            while ((indexPos = str.IndexOf(delimiter, startIndex)) >= 0)
            {
                if (IsAlignedDelimiterPos(alignedPos, indexPos, delimiter.Length) == false)
                {
                    // non-aligned delimiter found
                    return indexPos;
                }

                startIndex = indexPos + delimiter.Length;
            }

            // no non-aligned delimiter found
            return -1;
        }

        /// <summary>
        /// Determines whether the provided delimiter position matches an aligned position.
        /// </summary>
        /// <param name="alignedPos">The first position an aligned delimiter is expected.</param>
        /// <param name="pos">The current position of the delimiter.</param>
        /// <param name="delimiterLen">The length of the delimiter.</param>
        /// <returns><c>true</c> if delimiter position is aligned; otherwise, <c>false</c>.</returns>
        private static bool IsAlignedDelimiterPos(int alignedPos, int pos, int delimiterLen)
        {
            return (pos == 0) || ((pos >= alignedPos) &&
                ((((pos / alignedPos) - 1) * delimiterLen) == (pos % alignedPos)));
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current
        /// instance are replaced recursive with another specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of oldValue.</param>
        /// <returns>
        /// A string that is equivalent to the current string except that all instances of oldValue
        /// are replaced recursive with newValue. If oldValue is not found in the current instance,
        /// the method returns the current instance unchanged.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static string ReplaceRecursive(this string str, string oldValue, string newValue)
        {
            int strLen;
            do
            {
                strLen = str.Length;
                str = str.Replace(oldValue, newValue);
            } while (str.Length != strLen);

            return str;
        }
    }
}