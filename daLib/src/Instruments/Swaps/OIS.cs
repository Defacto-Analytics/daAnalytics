
using System;

using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.Model;
using daLib.DateUtils;
using System.Runtime.CompilerServices;
using System.Linq;

namespace daLib.Instruments.Swaps
{
    public class OIS : Swap, IEquatable<OIS>
    {
        // notation => fixed = leg1;
        //          => float = leg2;

        // Implement base constructor
        // Keep fixed/float notation in IRS constructor;
        public OIS(DateTime Start, DateTime Maturity, string DayRule, string FloatDayCount, string FixedDayCount,
                   Index FloatIndex, Index FixedIndex, BusinessCalendar calendar) :
        base(Start, Maturity, DayRule, FloatDayCount, FixedDayCount, FloatIndex, FixedIndex, calendar)
        {
        }

        // Implement default constructor
        public OIS() : base() { }
        private DateTime[] compoundInterest;
        

        // More effecient compound interest.
        private double CompoundInterest(DateTime Start, DateTime End, CurveModel model)
        {
            double rate = 1;

            int start_idx = Helper.LeftSegmentIndex(compoundInterest, Start);
            int end_idx = Helper.LeftSegmentIndex(compoundInterest, End);

            for (int i = start_idx; i < end_idx; i++)
            {
                 rate *= (1.0 + DateTimeUtils.Cvg(compoundInterest[i], compoundInterest[i+1], this.leg2_daycount) * model.Forward(leg2_index.getValue(), compoundInterest[i], compoundInterest[i + 1], this.leg2_daycount));
            }

            return System.Math.Pow(DateTimeUtils.Cvg(Start, End, leg2_daycount), -1) * (rate - 1);
        }

        private double CompoundInterest(DateTime Start, DateTime End, CurveModel model, string DayCount)
        {
            double rate = 1;

            DateTime tmp = new DateTime();
            DateTime tmp_next = new DateTime();
            tmp_next = Start;
            tmp = tmp_next;

            while (tmp < End)
            {
                tmp_next = DateTimeUtils.AddTenor(tmp_next, "1b", this.calendar, null);
                rate *= (1.0 + DateTimeUtils.Cvg(tmp, tmp_next, DayCount) * model.Forward(leg2_index.getValue(), tmp, tmp_next, DayCount));

                tmp = tmp_next;
            }

            return System.Math.Pow(DateTimeUtils.Cvg(Start, End, DayCount), -1) * (rate - 1);
        }

        public override double Price(CurveModel model)
        {
            InitCheck(model.Anchor);
            
            double tmp_fixed = CurveModelHelper.Annuity(model, leg1_schedule, leg1_daycount);
            double tmp_float = 0;

            foreach (var row in leg2_schedule.dates)
            {
                tmp_float += row.Cvg(leg2_daycount) * model.DiscFactor(row.adjEnd, leg2_daycount) * CompoundInterest(row.adjStart, row.adjEnd, model, leg2_daycount);
            }

            return tmp_float / tmp_fixed;
        }

        public override double NPV(CurveModel model, double fixed_rate)
        {
            InitCheck(model.Anchor);

            double tmp_fixed = CurveModelHelper.Annuity(model, leg1_schedule, leg1_daycount);
            double tmp_float = 0;

            foreach (var row in leg2_schedule.dates)
            {
                tmp_float += row.Cvg(leg2_daycount) * model.DiscFactor(row.adjEnd, leg2_daycount) * CompoundInterest(row.adjStart, row.adjEnd, model, leg2_daycount);
            }

            return tmp_float - fixed_rate * tmp_fixed;
        }

        public override void InitCheck(DateTime Anchor)
        {
            base.InitCheck(Anchor);
            if (compoundInterest == default(DateTime[]))
            {
                compoundInterest = DateTimeUtils.Schedule(this.unadjStart, this.unadjEnd , "1b", calendar);
            }
        }

        public override void RemoveTempObjects()
        {
            compoundInterest = null;
        }

        public bool Equals(OIS o)
        {
            return this.Equals((Swap)o);
        }

    }
}
