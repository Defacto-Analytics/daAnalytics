
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using daLib.Conventions;
using daLib.Conventions.Calenders;

namespace daLib.DateUtils
{
    public static class DateTimeUtils
    {
        // Public Date Utility Methods
        public static DateTime AddTenor(DateTime startDate, string tenor, string dayRule = "mf", string calendar = "weekends_only")
        {
            bool forward = true;
            BusinessCalendar c = new BusinessCalendar(calendar);

            DateTime endDate = DateTime.Now;

            // Identify type of tenor
            string tenorType = tenor[tenor.Length - 1].ToString().ToLower();

            // how many of typeTenor
            // i.e. how many days, weeks, months or years.
            int tenorLenght = Int32.Parse(tenor.Substring(0, tenor.Length - 1));

            // Find unadjusted anniversary/roll date
            endDate = tenorTypeAdjust(startDate, c, tenorType, tenorLenght);

            // Adjust according to dayrule
            // First check if we are adjusting backwards in businessdays - usually done to get EOD dates
            if ((tenorType == "b") && (tenorLenght < 0))
            {
                forward = false;
            }
            endDate = dayRuleAdjust(endDate, dayRule, c, forward);

            return endDate;

        }

        public static DateTime AddTenor(DateTime startDate, string tenor, BusinessCalendar calendar, string dayRule = "mf")
        {
            bool forward = true;

            DateTime endDate = DateTime.Now;

            // Identify type of tenor
            string tenorType = tenor[tenor.Length - 1].ToString().ToLower();

            // how many of typeTenor
            // i.e. how many days, weeks, months or years.
            int tenorLenght = Int32.Parse(tenor.Substring(0, tenor.Length - 1));

            // Find unadjusted anniversary/roll date
            endDate = tenorTypeAdjust(startDate, calendar, tenorType, tenorLenght);

            // Adjust according to dayrule
            // First check if we are adjusting backwards in businessdays - usually done to get EOD dates
            if ((tenorType == "b") && (tenorLenght < 0))
            {
                forward = false;
            }
            endDate = dayRuleAdjust(endDate, dayRule, calendar, forward);

            return endDate;

        }

        public static int CountBusinessDays(DateTime startDate, DateTime endDate, BusinessCalendar calendar)
        {
            int count = 0;
            DateTime tempDate = startDate;

            if (tempDate <= endDate)
            {
                while (tempDate < endDate)
                {
                    if (calendar.IsGoodBusinessDay(tempDate))
                    {
                        count += 1;
                    }

                    tempDate = tempDate.AddDays(1);
                }
            }
            else // if mistakenly switched start/end dates
            {
                while(endDate < tempDate)
                {
                    if(calendar.IsGoodBusinessDay(endDate))
                    {
                        count += 1;
                    }

                    endDate = endDate.AddDays(1);
                }
            }

            return count;
        }


        public static DateTime AdjustDate(DateTime startDate, string dayRule = "mf", string calendar = "weekends_only")
        {

            return AddTenor(startDate, "0b", dayRule, calendar);
        }

        public static DateTime AdjustDate(DateTime startDate, BusinessCalendar calendar, string dayRule = "mf")
        {
            
            return AddTenor(startDate, "0b", calendar, dayRule);
        }

        public static double Cvg(DateTime startDate, DateTime endDate, string dayCountBasis = "act/360")
        {
            double cvg = 0;

            switch (dayCountBasis.ToLower())
            {
                case "act/360":
                    cvg = (endDate.Date - startDate.Date).Days / 360.0;
                    break;
                case "act/365":
                    cvg = (endDate.Date - startDate.Date).Days / 365.0;
                    break;
                case "act/365.25":
                    cvg = (endDate.Date - startDate.Date).Days / 365.25;
                    break;
                case "30/360":
                    cvg = ((endDate.Year - startDate.Year) * 360.0
                            + (endDate.Month - startDate.Month) * 30.0
                            + System.Math.Min(30.0, endDate.Day) - System.Math.Min(30.0, startDate.Day)) / 360.0;
                    break;
            }

            return cvg;
        }

        // Private Auxillary Methods
        private static DateTime tenorTypeAdjust(DateTime startDate, BusinessCalendar calendar, string tenorType, int tenorLenght)
        {
            DateTime endDate = startDate;

            switch (tenorType)
            {
                case "d":
                    endDate = startDate.AddDays(tenorLenght);
                    break;
                case "b":
                    while(System.Math.Abs(CountBusinessDays(startDate,endDate, calendar)) != System.Math.Abs(tenorLenght))
                    {
                        endDate = endDate.AddDays(1 * System.Math.Sign(tenorLenght));
                    }
                    //endDate = startDate.AddDays(tenorLenght);
                    break;
                case "w":
                    endDate = startDate.AddDays(tenorLenght * 7.0);
                    break;
                case "m":
                    endDate = startDate.AddMonths(tenorLenght);
                    break;
                case "y":
                    endDate = startDate.AddYears(tenorLenght);
                    break;
                default:
                    throw new SystemException("This should never happen - check convention layer");
            }

            return endDate;
        }
        private static DateTime dayRuleAdjust(DateTime day, string dayRule, BusinessCalendar c, bool forward = true)
        {

            if (!forward)
            {
                dayRule = "p"; // If we are not adjusting forward, we are always adjusting using preceding
                // We use this when we calculating last good businessday for EOD model/rate purposes
                // i.e. on mondays AddTenor(today,-1b) should return friday before(or last good business day)
                // if we do not use preceding, AddTenor(today,-1b) would return monday which would be wrong.
            }

            switch (dayRule)
            {
                case "mf": // modified following

                    string unAdjustedMonth = day.Month.ToString(); // need this to check modified following rule

                    while (!c.IsGoodBusinessDay(day))
                    {
                        day = day.AddDays(1);
                    }

                    // last check for modified following to see if endDate is rolled into new month
                    // in which case we roll back to last good businessday in "current" month

                    if (day.Month.ToString() != unAdjustedMonth)
                    {
                        day = day.AddDays(-1);
                        while (!c.IsGoodBusinessDay(day))
                        {
                            day = day.AddDays(-1);
                        }
                    }
                    break;

                case "f": // following

                    while (!c.IsGoodBusinessDay(day))
                    {
                        day = day.AddDays(1);
                    }
                    break;

                case "p": // Preceding

                    while (!c.IsGoodBusinessDay(day))
                    {
                        day = day.AddDays(-1);
                    }
                    break;

                default:
                    break;

            }

            return day;
        }

        public static double DateTimeToSerial(DateTime d)
        {
            // Use excel double formatted dates in our curve setup;
            return d.ToOADate();

        }

        public static DateTime SerialToDateTime(double d)
        {
            return DateTime.FromOADate(d);
        }

        public static DateTime[] Schedule(DateTime start, DateTime end, string tenor, BusinessCalendar calender)
        {
            List<DateTime> res = new List<DateTime>();

            res.Add(start);
            DateTime tmp = DateTimeUtils.AddTenor(start, tenor, calender, null);
            while (tmp < end)
            {
                res.Add(tmp);
                tmp = DateTimeUtils.AddTenor(tmp, tenor, calender, null); 
            }
            res.Add(tmp);
            return res.ToArray();
        }

    }




    public class DateSchedule : IEquatable<DateSchedule>
    {

        public DateTime Anchor;
        public DateTime unadjStart;
        public DateTime unadjEnd;

        public string DayRule;
        public BusinessCalendar Calendar;
        public List<DateRow> dates;



        public DateSchedule()
        {
            this.dates = new List<DateRow>();
        }
        public DateSchedule(DateTime Start, DateTime End, BusinessCalendar Calendar, string Frequency = null, string DayRule = "mf")
        {
            this.unadjStart = Start;
            this.unadjEnd = End;
            this.Anchor = DateTime.MinValue;

            this.DayRule = DayRule;
            this.Calendar = Calendar;
            this.dates = new List<DateRow>();

            GenerateSchedule(Frequency);
        }

        // Used when pricing backdated swaps
        public void GetRelevantDates(DateTime Anchor)
        {
            List<DateRow> ForwardDates = new List<DateRow>();

            foreach (DateRow row in this.dates)
            {
                if (row.adjEnd > Anchor)
                {
                    ForwardDates.Add(row);
                }
            }

            this.dates = ForwardDates;
        }


        private void GenerateSchedule(string Frequency)
        {
            DateTime AdjStart = DateTimeUtils.AdjustDate(this.unadjStart, this.Calendar, this.DayRule);
            DateTime AdjMat = DateTimeUtils.AdjustDate(this.unadjEnd, this.Calendar, this.DayRule);

            if (Frequency == null || DateTimeUtils.AddTenor(AdjStart, "-" + Frequency, this.Calendar, this.DayRule) >= AdjMat)
            {
                this.dates.Add(GenerateRow(this.unadjStart, this.unadjEnd));
                return;
            }

            List<DateTime> TempAnchors = new List<DateTime>();
            int idx = 0;
            TempAnchors.Add(this.unadjEnd);

            while (DateTimeUtils.AddTenor(TempAnchors[idx], "-" + Frequency, this.Calendar, this.DayRule) > AdjStart)
            {
                DateTime potentialDate = DateTimeUtils.AddTenor(TempAnchors[idx], "-" + Frequency, this.Calendar, null); // Is this right
                TempAnchors.Add(potentialDate > AdjStart ? potentialDate : AdjStart);
                idx++;
            }

            this.dates.Add(GenerateRow(this.unadjStart, TempAnchors[TempAnchors.Count - 1]));
            for (int i = TempAnchors.Count - 1; i >= 1; i--)
            {
                this.dates.Add(GenerateRow(TempAnchors[i], TempAnchors[i - 1]));
            }
        }

        private DateRow GenerateRow(DateTime start, DateTime end)
        {
            DateRow res = new DateRow();

            res.unadjStart = start;
            res.unadjEnd = end;
            res.adjStart = DateTimeUtils.AdjustDate(start, this.Calendar, this.DayRule);
            res.adjEnd = DateTimeUtils.AdjustDate(end, this.Calendar, this.DayRule);

            return res;
        }

        public bool Equals(DateSchedule o)
        {
            


            return new Tuple<DateTime,DateTime,string,List<DateRow>,BusinessCalendar>(this.unadjStart, this.unadjEnd, this.DayRule, this.dates, this.Calendar) ==
                    new Tuple<DateTime, DateTime, string, List<DateRow>, BusinessCalendar>(o.unadjStart, o.unadjEnd, o.DayRule, o.dates, o.Calendar);
        }

    }

    public class DateRow : IEquatable<DateRow>
    {
        public DateTime unadjStart;
        public DateTime unadjEnd;
        public DateTime adjStart;
        public DateTime adjEnd;

        public DateRow() { }

        public double Cvg(string DayCountBasis)
        {
            return DateTimeUtils.Cvg(adjStart, adjEnd, DayCountBasis);
        }


        public bool Equals(DateRow o)
        {
            return new Tuple<DateTime, DateTime, DateTime, DateTime>(this.unadjStart, this.unadjEnd, this.adjStart, this.adjEnd) ==
                    new Tuple<DateTime, DateTime, DateTime, DateTime>(o.unadjStart, o.unadjEnd, o.adjStart, o.adjEnd);
        }
    }


}
