
using System;

using daLib.DateUtils;
using daLib.Exceptions;

namespace daLib.Conventions
{
    /*             ###  Allowable dates formats ###
     * (double) SerialDate  
     * (DateTime) Date
     * (string) tenorDate i.e. "2y" ==> Accomanied with Anchor
     * 
     * 
     *  NOTE: This class also checks if a SerialDate is disguised as a string;
     */

    public class ValidDate : IConvention<DateTime>
    {
        private DateTime date;
        private string additional_err_msg;

        public ValidDate(string date)
        {
            bool valid = false;
            if (isSerialDate(date))
            {
                this.date = ConvertSerialDate(date);
                valid = true;
            }

            if (isTenorDate(date))
            {
                additional_err_msg = "TenorString supplied without an anchor";
                this.Throw();
            }

            if (!valid) Throw();
        }

        public ValidDate(double date)
        {
            if (isSerialDate(date))
            {
                this.date = ConvertSerialDate(date);
            }
            else
            {
                additional_err_msg = "Expected excel serial date format";
                this.Throw();
            }
        }

        public ValidDate(DateTime date)
        {
            this.date = date;
        }

        public ValidDate(string date, DateTime Anchor)
        {
            bool valid = false;
            if (isSerialDate(date))
            {
                this.date = ConvertSerialDate(date);
                valid = true;
            }

            if (isTenorDate(date))
            {
                this.date = ConvertTenorDate(date, Anchor);
                valid = true;
            }

            if (!valid) Throw();
        }

        private DateTime ConvertTenorDate(string date, DateTime Anchor)
        {
            return DateTimeUtils.AddTenor(Anchor, date, dayRule: null) ;
        }
        private DateTime ConvertSerialDate(string d)
        {
            return ConvertSerialDate(Double.Parse(d));
        }
        private DateTime ConvertSerialDate(double d)
        {
            return DateTime.FromOADate(d);
        }
        public static bool isTenorDate(string date)
        {
            try
            {
                string tenorType = date[date.Length - 1].ToString().ToLower();
                int tenorLenght = Int32.Parse(date.Substring(0, date.Length - 1));
                if (!(tenorType == "b" || tenorType == "d" || tenorType == "w" || tenorType == "m" || tenorType == "y"))
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }
        private bool isSerialDate(string date)
        {
            try
            {
                double d = Double.Parse(date);
                DateTime _d = DateTime.FromOADate(d);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool isSerialDate(double date)
        {
            try
            {
                DateTime d = DateTime.FromOADate(date);
                return true;
            }
            catch
            {
                return false;
            }
        }


        #region IConvention interface

        public DateTime getValue()
        {
            return date;
        }

        public bool isValid()
        {
            // validation is done in constructor;
            return true;
        }

        public void Throw()
        {
            string msg = "Date type not supported";
            if (additional_err_msg != null)
            {
                msg += " <> " + additional_err_msg;
            }
            throw new ExcelException(msg);
        }
        #endregion
    }
}


