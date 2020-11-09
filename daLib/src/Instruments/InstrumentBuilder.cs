
using System;
using System.Collections.Generic;

using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.Exceptions;
using daLib.Instruments.Swaps;

namespace daLib.Instruments
{
    public static class InstrumentBuilder
    {
        public static Swap BuildSwapFromConvention(DateTime start, DateTime end, Index index)
        {
            Dictionary<string, object> swapConventions = GetSwapConventions(index); // Gets which swap type to build too; 
            return BuildSwapType(swapConventions, start, end);
        }

        public static Instrument GetInstrumentFromParsedString(DateTime Anchor, CalibrationInstrument parsedInstrument)
        { 
            
            Instrument instrument; // should be type instrument

            string type = parsedInstrument._type;
            ValidDate start = new ValidDate(parsedInstrument._strStart,Anchor);
            ValidDate tenor = new ValidDate(parsedInstrument._strTenor, start.getValue());
            Index index = parsedInstrument._index;
            ConventionLayer.Validate(start, tenor, index);

            Dictionary<string, object> Conventions;
            switch (type)
            {
                case "swap":
                    Conventions = GetSwapConventions(index); // Gets which swap type to build too; 
                    instrument = BuildSwapType(Conventions, start.getValue(), tenor.getValue());
                    break;

                default:
                    Conventions = GetSwapConventions(index); // Gets which swap type to build too; 
                    instrument = BuildSwapType(Conventions, start.getValue(), tenor.getValue());
                    //throw new ExcelException("Calibration instrument not supported");
                    break;
            }

            return instrument;

        }

        #region BuildingHelpers
        private static Swap BuildSwapType(Dictionary<string, object> conventions, DateTime start, DateTime end)
        {
            switch ((string)conventions["swaptype"])
            {
                case "irs":
                    return new IRS(start, end, (string)conventions["dayrule"], (string)conventions["floatdaycount"], (string)conventions["fixeddaycount"]
                                     , (Index)conventions["floatindex"], (Index)conventions["fixedindex"], (BusinessCalendar)conventions["calendar"]);

                case "ois":
                    return new OIS(start, end, (string)conventions["dayrule"], (string)conventions["floatdaycount"], (string)conventions["fixeddaycount"]
                                     , (Index)conventions["floatindex"], (Index)conventions["fixedindex"], (BusinessCalendar)conventions["calendar"]);

                default:
                    throw new ExcelException("Could not find instrument");
            }
        }
        private static Dictionary<string, object> GetSwapConventions(Index index)
        {

            var Conventions = new Dictionary<string, object>();

            string DayRule = "";
            BusinessCalendar Calendar = new BusinessCalendar(); // Set the real Calendar later;
            string FloatDayCount = "";
            string FixedDayCount = "";
            string swapType;
            Index floatIndex = index.DeepClone();
            Index fixedIndex = index.DeepClone();

            switch (index.currency)
            {
                case "eur":
                    Calendar = new TARGET2();
                    DayRule = "mf";
                    FloatDayCount = "act/360";
                    FixedDayCount = "30/360";
                    fixedIndex.setTenor("1y");


                    /* technically not nessacery, but for readability */
                    fixedIndex.currency = index.currency;
                    floatIndex.currency = index.currency;

                    break;

                case "dkk":
                    Calendar = new Denmark(); 
                    DayRule = "mf";
                    FloatDayCount = "act/360";
                    FixedDayCount = "30/360";
                    fixedIndex.setTenor("1y");


                    /* technically not nessacery, but for readability */
                    fixedIndex.currency = index.currency;
                    floatIndex.currency = index.currency;

                    break;

                default:
                    index.Throw(); // Index should have caught this before reaching this point;
                    break;
            }

            switch (index.name)
            {
                case "ois":
                    floatIndex.setTenor("1y");
                    swapType = "ois";
                    break;

                // Shoud be updated when adding more swap instruments;
                default:
                    // only correct as LIBOR index name is "6m" etc.
                    floatIndex.setTenor(index.name) ;
                    swapType = "irs";
                    break;
            }

            Conventions.Add("dayrule", DayRule);
            Conventions.Add("calendar", Calendar);
            Conventions.Add("floatdaycount", FloatDayCount);
            Conventions.Add("fixeddaycount", FixedDayCount);
            Conventions.Add("floatindex", floatIndex);
            Conventions.Add("fixedindex", fixedIndex);
            Conventions.Add("swaptype", swapType);

            return Conventions;

        }
       
        #endregion

    }
}
