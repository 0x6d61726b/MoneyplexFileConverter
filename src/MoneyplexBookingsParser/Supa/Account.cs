/*--------------------------------------------------------------------------------------------------
Project:     OnlineBankingDataConverter
Description: Account master data representation.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using Newtonsoft.Json;
using System.Collections.Generic;

namespace OnlineBankingDataConverter.Supa
{
    #region Enum definitions

    /// <summary>
    /// The account type code.
    /// </summary>
    public enum AccountTypeCode
    {
        /// <summary>
        /// Current or checking account
        /// </summary>
        [JsonProperty("CACC")]
        Checking,

        /// <summary>
        /// Cash, cash register
        /// </summary>
        [JsonProperty("CASH")]
        Cash,

        /// <summary>
        /// Securities Portfolio
        /// </summary>
        [JsonProperty("PRTF")]
        SecuritiesPortfolio,

        /// <summary>
        /// Fixed-Term Deposit
        /// </summary>
        [JsonProperty("DPST")]
        FixedTermDeposit,

        /// <summary>
        /// Credit Card
        /// </summary>
        [JsonProperty("CRDC")]
        CreditCard,

        /// <summary>
        /// PayPal Account
        /// </summary>
        [JsonProperty("PPAL")]
        PayPal,

        /// <summary>
        /// Cryptocurrency account
        /// </summary>
        [JsonProperty("CRYP")]
        Cryptocurrency,

        /// <summary>
        /// [DEPRECATED] Current or checking account
        /// </summary>
        /// <remarks>
        /// This code can be used as an alternative to <see cref="Checking" /> for compatibility
        /// with older versions.
        /// </remarks>
        [JsonProperty("GIRO")]
        Giro,
    }

    #endregion

    /// <summary>
    /// The representation of the master data of an account.
    /// </summary>
    /// <remarks>This class follows Account Information scheme of SUPA v3.2 specification.</remarks>
    public class Account
    {
        #region Account belongings

        /// <summary>
        /// The list of <see cref="Booking" /> that belong to this account.
        /// </summary>
        public List<Booking> Bookings;

        #endregion

        /// <summary>
        /// [Optional] Internal ID that uniquely identifies an account within a data store.
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }

        /// <summary>
        /// [Optional] Country in which the account is held.
        /// </summary>
        /// <remarks>Not the country in which the account holder is located.</remarks>
        [JsonProperty("AcctCtry")]
        public string AccountCountry { get; set; }

        /// <summary>
        /// [Conditional] The IBAN of the account.
        /// </summary>
        /// <remarks>Must be specified if the account has an IBAN.</remarks>
        [JsonProperty("AcctIBAN")]
        public string AccountIBAN { get; set; }

        /// <summary>
        /// [Mandatory] Bank-specific, national account number (for example a ten-digit German
        /// account number).
        /// </summary>
        /// <remarks>For credit card accounts, the credit card number can be set here.</remarks>
        [JsonProperty("AcctNo")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// [Conditional] SWIFT BIC of the account.
        /// </summary>
        /// <remarks>
        /// Must be specified if the account-holding bank defines a BIC for this account.
        /// </remarks>
        [JsonProperty("AcctBIC")]
        public string AccountBIC { get; set; }

        /// <summary>
        /// [Conditional] National bank code (German: 'Bankleitzahl') of the account.
        /// </summary>
        /// <remarks>Must be specified if the bank has a national bank code.</remarks>
        [JsonProperty("AcctBankCode")]
        public string AccountBankCode { get; set; }

        /// <summary>
        /// [Conditional] Account currency.
        /// </summary>
        /// <remarks>
        /// May only be omitted for securities accounts. This information is mandatory for all other
        /// account types.
        /// </remarks>
        [JsonProperty("AcctCcy")]
        public string AccountCurrency { get; set; }

        /// <summary>
        /// [Optional] Account name assigned by the user or an account name specified by the bank
        /// account product name.
        /// </summary>
        [JsonProperty("AcctNm")]
        public string AccountName { get; set; }

        /// <summary>
        /// [Optional] Account type.
        /// </summary>
        /// <remarks>The code</remarks>
        [JsonProperty("AcctTpCd")]
        public AccountTypeCode? AccountTypeCode { get; set; }

        /// <summary>
        /// [Optional] Name of the account holder.
        /// </summary>
        [JsonProperty("OwnrNm")]
        public string OwnerName { get; set; }
    }
}