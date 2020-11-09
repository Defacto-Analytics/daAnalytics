using daLib.Currencies;
using daLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace daLib.src.Currencies
{
    /*
     *  Holds FX data; can handle multiple currencies;
     * 
     * 
     */
    public class ccyManager
    {
        public List<Entry> data;

        public ccyManager()
        {
            data = new List<Entry>();
        }

        public void Add(DateTime time, ccyPair pair)
        {
            if (pair.HasValue())
            {
                data.Add(new Entry(time, pair));
                data.Sort();
                return;
            }

            throw new ExcelException("Adding ccyPair without value to ccy manager");
        }
        public void Add(Entry e)
        {
            if (e.pair.HasValue())
            {
                data.Add(e);
                data.Sort();
                return;
            }

            throw new ExcelException("Adding ccyPair without value to ccy manager");
        }

        public void Latest(ccyPair pair)
        {
            for (int i = data.Count-1; i >= 0 ; i--)
            {
                ccyComparison compare = pair.ccyCompare(data[i].pair);

                if (compare == ccyComparison.NotEqual)
                {
                    continue;
                }
                else if (compare == ccyComparison.CompletlyEqual)
                {
                    pair.value = data[i].pair.CcyInDomestic();
                }
                else
                {
                    pair.value = data[i].pair.CcyInForeign();
                }
            }
        }
    }
}
