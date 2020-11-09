
using System;

namespace daLib.Conventions.Calenders
{
    public class Denmark : BusinessCalendar
    {
        public Denmark() : base(Impl.Singleton) { }

        class Impl : WesternImpl
        {
            public static readonly Impl Singleton = new Impl();
            private Impl() { }

            public override string name() { return "denmark"; }
            public override bool IsGoodBusinessDay(DateTime date)
            {
                DayOfWeek w = date.DayOfWeek;
                int d = date.Day, dd = date.DayOfYear;
                int m = date.Month;
                int y = date.Year;
                int em = easterMonday(y);
                if (isWeekend(w)
                    // Maundy Thursday
                    || (dd == em - 4)
                    // Good Friday
                    || (dd == em - 3)
                    // Easter Monday
                    || (dd == em)
                    // General Prayer Day
                    || (dd == em + 25)
                    // Ascension
                    || (dd == em + 38)
                    // Day after Ascension (bank holiday after year 2008)
                    || (dd == em + 39 && date.Year > 2008)
                    // Whit Monday
                    || (dd == em + 49)
                    // New Year's Day
                    || (d == 1 && m == 1)
                    // Constitution Day, June 5th
                    || (d == 5 && m == 6)
                    // Christmas day
                    || (d == 24 && m == 12)
                    // Christmas
                    || (d == 25 && m == 12)
                    // Boxing Day
                    || (d == 26 && m == 12)
                    // New Year's Eve (bank holiday from 2003)
                    || (d == 31 && m == 12 && date.Year >= 2003))
                    return false;
                return true;
            }
        }
    }
}
