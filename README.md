# MoneyplexFileConverter & MoneyplexBookingsParser

This project provides (basic) functionality to convert account bookings exported by [moneyplex](https://matrica.de) to be importable by [Banking4](https://www.subsembly.com/banking4.html).

The functionality can be extended to import or export any file format, as long as proper importer/exporter classes/methods are provided.

## MoneyplexFileConverter 

The `MoneyplexFileConverter`  project is a simple demo project that needs to be adopted to your needs and is NOT an out-of-box solution. For demonstration purposes, a simple [moneyplex XML export](src/MoneyplexFileConverter/test-data/MoneyplexXmlExport.xml) is provided.

### moneyplex
Since moneyplex provides some SEPA data only as a single, non-interpreted purpose string, the purpose string should be post-processed and written into the corresponding internal properties. A post-processor for `Commerzbank`, `PSD Bank`/`DKB` and `Sparda-Bank BW` is provided with the demo application. However, it is necessary to check if the post-processor works for your account bookings.

Currently, **the only supported moneylex account/bookings import file format is XML**, since I encountered issues with incorrectly escaped CSV file contents related to split bookings present in moneyplex 20 (Build 24901) and moneyplex 25 beta (Build 24810) exports.

Please be aware that moneyplex 20 Basic/Professional DOES NOT export XML file format (as of 2024-03-27). However, XML file exports with moneyplex 25 beta worked fine for me.

### Banking4 import with split bookings
Since Banking4 v8.4 does not yet support import of split bookings (not even re-importing split bookings exported by Banking4 itself), the import process needs to follow these steps if split bookings shall be imported.

Note: You may try to import collective and individual bookings into a test account first, following the procedure described in "Banking4 import (without split bookings)" below to verify that all bookings have been exported, processed and imported, before running the recipe below.

**IMPORTANT**: It is assumed that the bank account has already been created and contains no unread bookings.

 1. **Make a backup of your Banking4 database!**
 2. Install [AutoHotkey v2](https://www.autohotkey.com).
 3. Copy/Paste processed split bookings content (e.g., `MoneyplexXmlExport.split.ahk.txt`) into AutoHotkey script [banking4-Splitbookings.ahk](AutoHotkey/banking4-Splitbookings.ahk) function `GetSplitBooking(booking)` and run this script (will be running in background mode).
 4. Import collective bookings (e.g., `MoneyplexXmlExport.collective.json`) into Banking4.
 5. Set a filter in Banking4 to display only unread bookings.
 6. Scroll up to the first booking, place the mouse cursor over the Name cell of the first booking and press `F5` on the keyboard.
 7. Wait until the AutoHotkey script has finished processing all bookings and DO NOT move the mouse cursor or hit any keyboard button. Just watch.
 8. Disable the Banking4 filter and double-check if the split bookings have been correctly imported.
 9. Import individual bookings (e.g., `MoneyplexXmlExport.individual.json`) into Banking4.
 10. Verify that all data have been imported correctly.



### Banking4 import without split bookings
When importing just individual and collective bookings (WITHOUT split bookings), no import restrictions apply.
It is assumed that the bank account has already been created.

 1. **Make a backup of your Banking4 database!**
 2. Import collective bookings (e.g., `MoneyplexXmlExport.collective.json`) into Banking4.
 3. Import individual bookings (e.g., `MoneyplexXmlExport.individual.json`) into Banking4.
 4. Verify that all data have been imported correctly.

## MoneyplexBookingsParser

The `MoneyplexBookingsParser` is the library that imports the moneyplex XML exports and converts the account and booking data to internal `Account` and `Booking` objects that follows the "SUPA - Subsembly Payments" format.

In addition, this library provides converters to import/export bookings in `Banking4` SUPA JSON file format as well as the native .NET XML file format.

### Internal data format
It was decided to use the [SUPA - Subsembly Payments](https://www.subsembly.com/download/SUPA.pdf) data format, that is clearly specified and contains all data required for conversion to any target format. A copy of this specification is contained in this repository at [doc/SUPA_Specification_v3-2_DE.pdf](doc/SUPA_Specification_v3-2_DE.pdf) and an auto-translated English version at [doc/SUPA_Specification_v3-2_EN.pdf](doc/SUPA_Specification_v3-2_EN.pdf).

## License

[GPL-3.0 license](LICENSE)