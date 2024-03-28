/*--------------------------------------------------------------------------------------------------
Project:     OnlineBankingDataConverter
Description: Account booking data representation.
Copyright:   (c) 2024 Mark <0x6d61726b@gmail.com>
License:     [GPL-3.0-only] GNU General Public License v3.0 only
             https://www.gnu.org/licenses/gpl-3.0-standalone.html
--------------------------------------------------------------------------------------------------*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBankingDataConverter.Supa
{
    #region Enum definitions

    /// <summary>
    /// The booking status.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingStatus
    {
        /// <summary>
        /// Booked turnover.
        /// </summary>
        [EnumMember(Value = "BOOK")]
        Booked,

        /// <summary>
        /// Information turnover.
        /// </summary>
        /// <remarks>
        /// The associated booking is for information purposes only. No turnover has been booked for
        /// this account.
        /// </remarks>
        [EnumMember(Value = "INFO")]
        Information,

        /// <summary>
        /// Pending turnover.
        /// </summary>
        /// <remarks>
        /// The associated booking is not yet final. This status can occur in the case of notified
        /// sales.
        /// </remarks>
        [EnumMember(Value = "PDNG")]
        Pending,
    }

    /// <summary>
    /// The credit/debit indicator.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CreditDebit
    {
        /// <summary>
        /// Credit booking.
        /// </summary>
        [EnumMember(Value = "CRDT")]
        Credit,

        /// <summary>
        /// Debit booking.
        /// </summary>
        [EnumMember(Value = "DBIT")]
        Debit,
    }

    /// <summary>
    /// The flag indicator.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Flag
    {
        /// <summary>
        /// No special flag has been assigned
        /// </summary>
        [EnumMember(Value = "None")]
        None,

        /// <summary>
        /// The booking was marked as 'Done' by the user.
        /// </summary>
        [EnumMember(Value = "OK")]
        Ok,

        /// <summary>
        /// The booking was marked with red color by the user.
        /// </summary>
        [EnumMember(Value = "Red")]
        Red,

        /// <summary>
        /// The booking was marked with blue color by the user.
        /// </summary>
        [EnumMember(Value = "Blue")]
        Blue,

        /// <summary>
        /// The booking was marked with yellow color by the user.
        /// </summary>
        [EnumMember(Value = "Yellow")]
        Yellow,

        /// <summary>
        /// The booking was marked with green color by the user.
        /// </summary>
        [EnumMember(Value = "Green")]
        Green,

        /// <summary>
        /// The booking was marked with gray color by the user.
        /// </summary>
        [EnumMember(Value = "Gray")]
        Gray,

        /// <summary>
        /// The booking was marked with purple color by the user.
        /// </summary>
        [EnumMember(Value = "Purple")]
        Purple,
    }

    #endregion

    /// <summary>
    /// The representation of an account booking (transaction).
    /// </summary>
    /// <remarks>This class follows Account Turnover scheme of SUPA v3.2 specification.</remarks>
    [Serializable]
    public class Booking
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Booking" /> class.
        /// </summary>
        public Booking()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Booking" /> class and sets the owner
        /// account information based on the account it belongs to.
        /// </summary>
        /// <param name="account">The account this booking belongs to.</param>
        public Booking(Account account)
        {
            // set owner information based on the provided account
            if (account != null)
            {
                SetOwnerAccountInformation(account);
            }
        }

        /// <summary>
        /// [Optional] Internal ID that uniquely identifies a booking item within a data store.
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }

        /// <summary>
        /// [Optional] Internal ID of the account to which this booking item belongs to.
        /// </summary>
        /// <remarks>
        /// The <see cref="AccountId" /> must match the corresponding <see cref="Account.Id" />
        /// property.
        /// </remarks>
        [JsonProperty("AcctId")]
        public string AccountId { get; set; }

        /// <summary>
        /// [Optional] Account currency of the account to which this entry belongs.
        /// </summary>
        /// <remarks>Used if required for the purpose of clear identification.</remarks>
        [JsonProperty("OwnrAcctCcy")]
        public string OwnerAccountCurrency { get; set; }

        /// <summary>
        /// [Optional] IBAN of the account to which this booking belongs.
        /// </summary>
        /// <remarks>
        /// To identify the account, either the <see cref="OwnerAccountIBAN" /> property or the <see
        /// cref="OwnerAccountNumber" /> can be assigned.
        /// </remarks>
        [JsonProperty("OwnrAcctIBAN")]
        public string OwnerAccountIBAN { get; set; }

        /// <summary>
        /// [Optional] Bank-specific, national account number of the account to which this booking
        /// belongs.
        /// </summary>
        /// <remarks>
        /// To identify the account, either the <see cref="OwnerAccountIBAN" /> property or the <see
        /// cref="OwnerAccountNumber" /> can be assigned.
        /// </remarks>
        [JsonProperty("OwnrAcctNo")]
        public string OwnerAccountNumber { get; set; }

        /// <summary>
        /// [Optional] BIC of the bank of the account to which this booking belongs.
        /// </summary>
        /// <remarks>
        /// To identify the account, either the <see cref="OwnerAccountBIC" /> property or the <see
        /// cref="OwnerAccountBankCode" /> can be assigned.
        /// </remarks>
        [JsonProperty("OwnrAcctBIC")]
        public string OwnerAccountBIC { get; set; }

        /// <summary>
        /// [Optional] National bank code (German: 'Bankleitzahl') of the account to which this
        /// booking belongs.
        /// </summary>
        /// <remarks>
        /// To identify the account, either the <see cref="OwnerAccountBIC" /> property or the <see
        /// cref="OwnerAccountBankCode" /> can be assigned.
        /// </remarks>
        [JsonProperty("OwnrAcctBankCode")]
        public string OwnerAccountBankCode { get; set; }

        /// <summary>
        /// [Conditional] Booking date.
        /// </summary>
        /// <remarks>
        /// Must be used for bookings with <see cref="BookingStatus.Booked" />, but optional for all
        /// other <see cref="BookingStatus" />.
        /// </remarks>
        [JsonProperty("BookgDt")]
        public DateTime? BookingDate { get; set; }

        /// <summary>
        /// [Conditional] Value date.
        /// </summary>
        /// <remarks>
        /// Must be used for bookings with <see cref="BookingStatus.Booked" />, but optional for all
        /// other <see cref="BookingStatus" />.
        /// </remarks>
        [JsonProperty("ValDt")]
        public DateTime? ValueDate { get; set; }

        /// <summary>
        /// [Optional] Date of the customer transaction or voucher date.
        /// </summary>
        /// <remarks>
        /// For example, the date a card payment at a checkout. This property is mainly used for
        /// credit card transactions.
        /// </remarks>
        [JsonProperty("TxDt")]
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// [Mandatory] Amount posted to the account.
        /// </summary>
        [JsonProperty("Amt")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// [INTERNAL] Amount posted to the account as signed value.
        /// </summary>
        [JsonIgnore]
        public decimal? AmountSigned
        {
            get
            {
                if ((Amount != null) && (CreditDebitIndicator != null))
                {
                    decimal amountSigned = Amount.Value;
                    if (CreditDebitIndicator.Value == CreditDebit.Debit)
                    {
                        amountSigned *= -1;
                    }

                    return amountSigned;
                }

                return null;
            }

            set
            {
                decimal valueSigned = (value ?? 0);

                Amount = Math.Abs(valueSigned);
                if (valueSigned < 0)
                {
                    CreditDebitIndicator = CreditDebit.Debit;
                }
                else
                {
                    CreditDebitIndicator = CreditDebit.Credit;
                }
            }
        }

        /// <summary>
        /// [Mandatory] Currency of the posted amount.
        /// </summary>
        /// <remarks>
        /// This currency must always correspond to the account currency at the time of posting.
        /// </remarks>
        [JsonProperty("AmtCcy")]
        public string AmountCurrency { get; set; }

        /// <summary>
        /// [Mandatory] Credit/Debit indicator.
        /// </summary>
        [JsonProperty("CdtDbtInd")]
        public CreditDebit? CreditDebitIndicator { get; set; }

        /// <summary>
        /// [Optional] Amount originally ordered.
        /// </summary>
        /// <remarks>
        /// Only to be used if this differs from the amount booked. If this property is filled, the
        /// <see cref="InstructedAmountCurrency" /> property must also be filled. <br /> The
        /// Credit/Debit indicator is given by the <see cref="CreditDebitIndicator" /> property.
        /// </remarks>
        [JsonProperty("InstdAmt")]
        public decimal? InstructedAmount { get; set; }

        /// <summary>
        /// [Conditional] Currency of the originally ordered amount.
        /// </summary>
        /// <remarks>
        /// To be used if the <see cref="InstructedAmount" /> property has been used and must not be
        /// used otherwise. <br /> This currency must always correspond to the account currency at
        /// the time of posting.
        /// </remarks>
        [JsonProperty("InstdAmtCcy")]
        public string InstructedAmountCurrency { get; set; }

        /// <summary>
        /// [Optional] Total amount of charges incl. taxes.
        /// </summary>
        /// <remarks>Only to be used if individual charges denominated in same currency.</remarks>
        [JsonProperty("TtlChrgsAndTaxAmt")]
        public decimal? TotalChargesAndTaxAmount { get; set; }

        /// <summary>
        /// [Conditional] Currency of the total amount of charges in <see
        /// cref="TotalChargesAndTaxAmount" /> property.
        /// </summary>
        /// <remarks>
        /// To be used if the <see cref="TotalChargesAndTaxAmount" /> property has been used and
        /// must not be used otherwise.
        /// </remarks>
        [JsonProperty("TtlChrgsAndTaxAmtCcy")]
        public string TotalChargesAndTaxAmountCurrency { get; set; }

        /// <summary>
        /// [Optional] End-to-End reference (EREF) for SEPA payments or customer number for DTA
        /// (data carrier exchange) payments.
        /// </summary>
        [JsonProperty("EndToEndId")]
        public string EndToEndId { get; set; }

        /// <summary>
        /// [Optional] Customer reference (KREF) for SEPA payments, reference number of the
        /// submitter for DTA (data carrier exchange) payments.
        /// </summary>
        [JsonProperty("PmtInfId")]
        public string PaymentInformationId { get; set; }

        /// <summary>
        /// [Optional] Mandate reference (MREF) for SEPA direct debits.
        /// </summary>
        [JsonProperty("MndtId")]
        public string MandateId { get; set; }

        /// <summary>
        /// [Optional] Creditor Identifier (CRED) for SEPA direct debits.
        /// </summary>
        [JsonProperty("CdtrId")]
        public string CreditorId { get; set; }

        /// <summary>
        /// [Optional] Intended use.
        /// </summary>
        /// <remarks>
        /// Must not contain line breaks. An original multi-line purpose is merged into a single
        /// line by concatenation.
        /// </remarks>
        [JsonProperty("RmtInf")]
        public string RemittanceInformation { get; set; }

        /// <summary>
        /// [Optional] Four character purpose code.
        /// </summary>
        /// <remarks>
        /// Example values are: 'BONU', 'PENS', 'SALA', 'CBFF', 'GOVT', 'SSBE', 'BENE', 'CHAR', ...
        /// </remarks>
        [JsonProperty("PurpCd")]
        public string PurposeCode { get; set; }

        /// <summary>
        /// [Optional] Booking text.
        /// </summary>
        /// <remarks>The booking text describes the type of payment, e.g. 'TRANSFER'.</remarks>
        [JsonProperty("BookgTxt")]
        public string BookingText { get; set; }

        /// <summary>
        /// [Optional] Prima nota number.
        /// </summary>
        [JsonProperty("PrimaNotaNo")]
        public string PrimaNotaNumber { get; set; }

        /// <summary>
        /// [Optional] Bank reference.
        /// </summary>
        /// <remarks>
        /// When importing/exporting PayPal transactions, the PayPal transaction ID should be set in
        /// this field.
        /// </remarks>
        [JsonProperty("BankRef")]
        public string BankReference { get; set; }

        /// <summary>
        /// [Optional] Four character booking code.
        /// </summary>
        /// <remarks>
        /// Example values are: 'NMSC', 'NTRF', 'NDDT', 'NCLR', 'NCHK', 'NSTO', 'NRTI', ...
        /// </remarks>
        [JsonProperty("BookgTxCd")]
        public string BookingCode { get; set; }

        /// <summary>
        /// [Optional] Name of the payee or payer (account holder).
        /// </summary>
        [JsonProperty("RmtdNm")]
        public string RemittedName { get; set; }

        /// <summary>
        /// [Optional] Name of the payee or payer (informational).
        /// </summary>
        [JsonProperty("RmtdUltmtNm")]
        public string RemittedUltimateName { get; set; }

        /// <summary>
        /// [Optional] Country of the payee or payer.
        /// </summary>
        [JsonProperty("RmtdAcctCtry")]
        public string RemittedAccountCountry { get; set; }

        /// <summary>
        /// [Optional] IBAN of the payee or payer.
        /// </summary>
        [JsonProperty("RmtdAcctIBAN")]
        public string RemittedAccountIBAN { get; set; }

        /// <summary>
        /// [Optional] Bank-specific, national account number of the payee or payer.
        /// </summary>
        [JsonProperty("RmtdAcctNo")]
        public string RemittedAccountNumber { get; set; }

        /// <summary>
        /// [Optional] BIC of the payee or payer.
        /// </summary>
        [JsonProperty("RmtdAcctBIC")]
        public string RemittedAccountBIC { get; set; }

        /// <summary>
        /// [Optional] National bank code (German: 'Bankleitzahl') of the payee or payer.
        /// </summary>
        [JsonProperty("RmtdAcctBankCode")]
        public string RemittedAccountBankCode { get; set; }

        /// <summary>
        /// [Optional] Booking status of this entry.
        /// </summary>
        /// <remarks>
        /// If this is not specified, <see cref="BookingStatus.Booked" /> is implicitly assumed as
        /// the status.
        /// </remarks>
        [JsonProperty("BookgSts")]
        public BookingStatus? BookingStatus { get; set; }

        /// <summary>
        /// [Optional] The three-digits (with leading zeros) business transaction code.
        /// </summary>
        /// <remarks>
        /// Business transaction code according to the 'Specification of Data Formats' of the German
        /// Banking Industry in chapter MT 940 Account Statement Data section 'Business Transaction
        /// Codes'.
        /// </remarks>
        [JsonProperty("GVC")]
        public string BusinessTransactionCode { get; set; }

        /// <summary>
        /// [Optional] The three-digits (with leading zeros) text key supplement of the business
        /// transaction code.
        /// </summary>
        /// <remarks>
        /// Text key supplement according to the 'Specification of Data Formats' of the German
        /// Banking Industry in chapter MT 940 Account Statement Data section 'Conversion of SEPA
        /// codes in field 86 (subfield 34)'.
        /// </remarks>
        [JsonProperty("GVCExtension")]
        public string BusinessTransactionCodeExtension { get; set; }

        /// <summary>
        /// [Optional] Return information reason code.
        /// </summary>
        /// <remarks>
        /// In the case of a R transaction (return) the code(ExternalReturnReason1Code) for the
        /// return reason can be set.
        /// </remarks>
        [JsonProperty("RtrInfRsnCd")]
        public string ReturnInformationReasonCode { get; set; }

        /// <summary>
        /// [Optional] Indicates whether this is a collective booking or a partial booking of a
        /// collective booking.
        /// </summary>
        /// <remarks>
        /// If the partial bookings contained in a collective booking file are also exported, the
        /// collective booking must always be placed immediately before the partial bookings it
        /// contains. <br /> If partial bookings are exported, all partial bookings of a collective
        /// booking must always be exported. The total amount of the partial bookings must equal the
        /// total amount of the higher-level collective booking.
        /// </remarks>
        /// <returns>
        /// <c>true</c> for a collective booking; <c>false</c> for a partial booking; <c>null</c>
        /// for a normal individual booking.
        /// </returns>
        [JsonProperty("BtchBookg")]
        public bool? BatchBooking { get; set; }

        /// <summary>
        /// [Optional] Internal ID (see <see cref="Id" />) of the higher-level collective booking.
        /// </summary>
        /// <remarks>
        /// This property may only be used for partial bookings of a collective booking. In this
        /// case, the property <see cref="BatchBooking" /> must be set to <c>false</c> in order to
        /// mark the data record as a partial booking. <br /> It is permitted that only partial
        /// bookings are exported without the higher-level collective bookings. In this case, the
        /// property <see cref="BatchBooking" /> contains the value <c>false</c> and the (see <see
        /// cref="BatchId" />) is omitted or <c>null</c>.
        /// </remarks>
        [JsonProperty("BtchId")]
        public string BatchId { get; set; }

        /// <summary>
        /// [Optional] Application-specific category assigned to this booking.
        /// </summary>
        /// <remarks>A colon is used as a separator to create a hierarchy of categories.</remarks>
        [JsonProperty("Category")]
        public string Category { get; set; }

        /// <summary>
        /// [Optional] The date selected by the user for an evaluation of the booking according to
        /// the selected category.
        /// </summary>
        [JsonProperty("CategoryDt")]
        public DateTime? CategoryDate { get; set; }

        /// <summary>
        /// [Optional] Notes freely assigned by the user for this booking.
        /// </summary>
        [JsonProperty("Notes")]
        public string Notes { get; set; }

        /// <summary>
        /// [Optional] Indicates whether this booking has already been read by the user.
        /// </summary>
        [JsonProperty("ReadStatus")]
        public bool? ReadStatus { get; set; }

        /// <summary>
        /// [Optional] An indicator selected by the user for this booking.
        /// </summary>
        [JsonProperty("Flag")]
        public Flag? Flag { get; set; }

        /// <summary>
        /// [Optional] The VAT amount contained in the booking amount.
        /// </summary>
        /// <remarks>
        /// With 19 % VAT and a booking amount of 100.00 EUR, the amount of value added tax share
        /// saved here would be 15.97 EUR.
        /// </remarks>
        [JsonProperty("VatAmt")]
        public decimal? ValueAddedTaxAmount { get; set; }

        /// <summary>
        /// Sets the owner account information for this booking based on the account it belongs to.
        /// </summary>
        /// <param name="account">The account this booking belongs to.</param>
        public void SetOwnerAccountInformation(Account account)
        {
            AccountId = account.Id;
            OwnerAccountBankCode = account.AccountBankCode;
            OwnerAccountBIC = account.AccountBIC;
            OwnerAccountCurrency = account.AccountCurrency;
            OwnerAccountIBAN = account.AccountIBAN;
            OwnerAccountNumber = account.AccountNumber;
        }

        #region Helpers

        /// <summary>
        /// The default JSON serializer settings.
        /// </summary>
        internal static JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd",
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Calculates the hash identifier of this instance.
        /// </summary>
        /// <param name="count">
        /// The optional counter value to be used in case of identical bookings on the same day.
        /// </param>
        /// <returns>The hash identifier of this instance.</returns>
        public string CalculateHashId(int count = 0)
        {
            // make a copy of the current object
            string jsonStr = JsonConvert.SerializeObject(this, JsonSerializerSettings);
            Booking booking = JsonConvert.DeserializeObject<Booking>(jsonStr);

            // clear the ID field
            booking.Id = null;

            // re-create the JSON string
            jsonStr = JsonConvert.SerializeObject(booking, Formatting.None, JsonSerializerSettings);

            // create hash from JSON string
            using (SHA1 hash = SHA1.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.Default.GetBytes(jsonStr));
                return Convert.ToBase64String(bytes).TrimEnd('=') + "-" + count.ToString();
            }
        }

        #endregion
    }
}