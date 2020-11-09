using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.Instruments;
using System;


namespace daAnalytics.Instruments.SingleCashflows
{
    public abstract class SingleCashflow : Instrument
    {
        public string DayCount;
        public Index baseIndex;

        protected SingleCashflow(DateTime Start, DateTime Maturity, Index index, string DayRule, string DayCount, BusinessCalendar calendar) : base()
        {
            this.unadjStart = Start;
            this.unadjEnd = Maturity;
            this.DayRule = DayRule;
            this.DayCount = DayCount;
            this.baseIndex = index;
            this.calendar = calendar;

            index.CheckForTenor();
        }

        /*
        public bool Equals(SingleCashflow o)
        {
            return (this.unadjStart, this.unadjEnd, this.baseIndex, this.DayRule, this.DayCount) ==
                    (o.unadjStart, o.unadjEnd, o.baseIndex, o.DayRule, o.DayCount);
        }
        */
    }
}
