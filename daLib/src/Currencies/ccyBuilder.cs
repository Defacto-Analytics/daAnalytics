using daLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace daLib.Currencies
{
    public static class ccyBuilder
    {

        public static ccyPair BuildPairParse(string pair, double value)
        {
            string pattern = @"[\\/:|]";
            string[] split_pair = Regex.Split(pair, pattern, RegexOptions.IgnoreCase);

            if (split_pair.Length != 2)
            {
                throw new ExcelException("Could not parse currencypair");
            }

            Currency domccy = new Currency(split_pair[0]);
            Currency forccy = new Currency(split_pair[1]);

            return new ccyPair(domccy,forccy,value);
        }

        public static Entry BuildEntryParse(string pair, double value)
        {
            return new Entry(DateTime.Now, BuildPairParse(pair, value));
        }

        public static Entry BuildEntryParse(string pair, double value, DateTime time)

        {
            return new Entry(time, BuildPairParse(pair, value));
        }
    }
}
