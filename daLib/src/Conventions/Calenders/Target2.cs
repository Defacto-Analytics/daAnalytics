
using System;

namespace daLib.Conventions.Calenders
{
    public class TARGET2 : BusinessCalendar
    {
        public TARGET2() : base(Impl.Singleton) { }

        class Impl : WesternImpl
        {
            internal static readonly Impl Singleton = new Impl();
            private Impl() { }

            public override string name() { return "target2"; }
            public override bool IsGoodBusinessDay(DateTime date)
            {
                DayOfWeek w = date.DayOfWeek;
                int d = date.Day, dd = date.DayOfYear;
                int m = date.Month;
                int y = date.Year;
                int em = easterMonday(y);

                if (isWeekend(w)
                    // New Year's Day
                    || (d == 1 && m == 1)
                    // Good Friday
                    || (dd == em - 3 && y >= 2000)
                    // Easter Monday
                    || (dd == em && y >= 2000)
                    // Labour Day
                    || (d == 1 && m == 5 && y >= 2000)
                    // Christmas
                    || (d == 25 && m == 12)
                    // Day of Goodwill
                    || (d == 26 && m == 12 && y >= 2000)
                    // December 31st, 1998, 1999, and 2001 only
                    || (d == 31 && m == 12 && (y == 1998 || y == 1999 || y == 2001)))
                    return false;
                return true;
            }

        }
    }
}
