
using System;

using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.DateUtils;

namespace daLib.Instruments.Swaps
{


    public abstract class Swap : Instrument, IEquatable<Swap>
    {
        public Index leg1_index;
        public Index leg2_index;

        public string leg1_daycount;
        public string leg2_daycount;

        public DateSchedule leg1_schedule;
        public DateSchedule leg2_schedule;

        public Swap() : base() { }
        public Swap(DateTime Start, DateTime Maturity, string DayRule, string leg2_DayCount, string leg1_DayCount,
                    Index leg2_Index, Index leg1_Index, BusinessCalendar calendar) : base()
        {
            this.DayRule = DayRule;
            this.calendar = calendar;

            this.leg1_daycount = leg1_DayCount;
            this.leg2_daycount = leg2_DayCount;

            this.leg1_index = leg1_Index;
            this.leg2_index = leg2_Index;

            this.unadjStart = Start;
            this.unadjEnd = Maturity;

            this.leg1_index.CheckForTenor();
            this.leg2_index.CheckForTenor();

            this.leg1_schedule = new DateSchedule(this.unadjStart, this.unadjEnd, this.calendar, this.leg1_index.tenor, this.DayRule);
            this.leg2_schedule = new DateSchedule(this.unadjStart, this.unadjEnd, this.calendar, this.leg2_index.tenor, this.DayRule);
        }

        public virtual void GenerateSchedules(DateTime Anchor)
        {
            // This can be moved into constructor of dateschedule, but here for visibility in case of bugs
            // Removes adjDates if before anchor date - for valuation of backdated swaps.
            this.leg1_schedule = new DateSchedule(this.unadjStart, this.unadjEnd, this.calendar, this.leg1_index.tenor, this.DayRule);
            this.leg2_schedule = new DateSchedule(this.unadjStart, this.unadjEnd, this.calendar, this.leg2_index.tenor, this.DayRule);

            // Used for pricing when anchor is after starting date
            this.leg1_schedule.GetRelevantDates(Anchor);
            this.leg2_schedule.GetRelevantDates(Anchor);

            this.leg1_schedule.Anchor = Anchor;
            this.leg1_schedule.Anchor = Anchor;
        }

        public virtual void InitCheck(DateTime Anchor)
        {
            if (Anchor != leg1_schedule.Anchor)
                GenerateSchedules(Anchor);
        }


        public bool Equals(Swap o)
        {
            return new Tuple<Index, Index, DateSchedule, DateSchedule, string, string, string,BusinessCalendar>(this.leg1_index, this.leg2_index, this.leg1_schedule, this.leg2_schedule, this.DayRule, this.leg1_daycount, this.leg2_daycount, this.calendar) ==
                    new Tuple<Index, Index, DateSchedule, DateSchedule, string, string, string, BusinessCalendar>(o.leg1_index, o.leg2_index, o.leg1_schedule, o.leg2_schedule, o.DayRule, o.leg1_daycount, o.leg2_daycount, this.calendar);
        }


        public override string ToString()
        {
            // Check if model instrument or portfolio/random instrument;
            string dateRepresentation = dateString == null ? this.DateToString(this.unadjStart) + " " + this.DateToString(this.unadjEnd) : dateString.ToUpper();
            return "SWAP " + dateRepresentation + " " + this.leg2_index.getValue().ToUpper();
        }

        protected override void HandleClone(Instrument clone)
        {


        }

    }
}
