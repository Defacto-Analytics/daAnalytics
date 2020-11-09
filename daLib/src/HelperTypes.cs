using daLib.Conventions;
using daLib.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace daLib
{
    public class CalibrationInstrument
    {
        public string _type;
        public string _strStart;
        public string _strTenor;
        public Index _index;
        public double _quote;

        public CalibrationInstrument(string instrument) { ParseInstrument(instrument); }
        public CalibrationInstrument(string instrument, double quote)
        {
            ParseInstrument(instrument);
            setQuote(quote);
        }
        public void setQuote(double quote)
        {
            this._quote = quote;
        }
        public void ParseInstrument(string instrument)
        {
            // Parse input instrument such as SWAP 10Y EUR6M into type:SWAP, tenor: 10y, index: EUR6M fx

            string[] inputs = instrument.Split(' ');
            int inputLenght = inputs.Length;

            // Check type first
            string type = inputs[0].ToLower();

            switch (type)
            {
                case "swap":
                    _type = "swap";
                    // check if at market or off market swap
                    // if off market, the last element of the input should be the rate
                    // that is, if the last element can be converted to double, it is a rate and therefore an off market swap
                    if (Helper.isNumeric(inputs.Last()))
                    {
                        // Last input is numeric, so we assume it is an off market instrument
                        // Next step is to check whether is is a spot starting or forward starting instrument
                        // we do this by counting the number of input arguments

                        if (inputLenght == 4)
                        {
                            // spot starting
                            _strStart = "2b";
                            _strTenor = inputs[1];
                            _index = new Index(inputs[2]);
                        }
                        else if (inputLenght == 5)
                        {
                            // forward starting

                            _strStart = inputs[1];
                            _strTenor = inputs[2];
                            _index = new Index(inputs[3]);
                        }
                        else
                        {
                            // Throw exception here
                        }

                    }
                    else
                    {
                        // Last input is not numeric, so we assume it is an at market instrument
                        // Next step is to checke whether it is a spot starting or forward starting insturment
                        // we do this by counting the number of input arguments

                        if (inputLenght == 3)
                        {
                            // spot starting 
                            _strStart = "2b";
                            _strTenor = inputs[1];
                            _index = new Index(inputs[2]);
                        }
                        else if (inputLenght == 4)
                        {
                            _strStart = inputs[1];
                            _strTenor = inputs[2];
                            _index = new Index(inputs[3]);
                        }
                        else
                        {
                            // Throw Exception here
                        }
                    }

                    break;
                case "bond":
                    break;
                default:
                    break; // Throw exception here
            }
        }

        public bool InstrumentEqual(CalibrationInstrument o)
        {
            if (this._type.Equals(o._type) && this._strStart.Equals(o._strStart) && this._strTenor.Equals(o._strTenor) && this._index.Equals(o._index))
            {
                return true;
            }
            return false;
        }
        public bool QuotetEqual(CalibrationInstrument o)
        {
            if (this._index.Equals(o._index))
            {
                return true;
            }
            return false;
        }

        public bool Equals(CalibrationInstrument o)
        {
            if (this._type.Equals(o._type) && this._strStart.Equals(o._strStart) && this._strTenor.Equals(o._strTenor) && this._index.Equals(o._index) && this._quote.Equals(o._quote))
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{_type} {_strStart} {_strTenor} {_index.getValue()}".ToUpper(); 
        }

    }

    public struct Point : IComparable<Point>
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public int CompareTo(Point p)
        {
            return this.x.CompareTo(p.x);
        }
    }
    public class Entry
    {
        public DateTime time;
        public ccyPair pair;

        public Entry(DateTime time, ccyPair pair)
        {
            this.time = time;
            this.pair = pair;
        }

        public int CompareTo(Entry e)
        {
            return this.time.CompareTo(e.time);
        }
    }

}
