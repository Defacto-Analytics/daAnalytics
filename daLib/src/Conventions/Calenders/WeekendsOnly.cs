
using System;

namespace daLib.Conventions.Calenders
{
    public class WeekendsOnly : BusinessCalendar
    {
        public WeekendsOnly() : base(Impl.Singleton) { }

        class Impl : WesternImpl
        {
            public static readonly Impl Singleton = new Impl();
            private Impl() { }

            public override string name() { return "weekends_only"; }
            public override bool IsGoodBusinessDay(DateTime date) { return !isWeekend(date.DayOfWeek); }

        }
    }
}
