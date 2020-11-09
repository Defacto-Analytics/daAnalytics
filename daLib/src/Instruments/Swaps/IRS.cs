
using System;

using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.Model;


namespace daLib.Instruments.Swaps
{
    public class IRS : Swap, IEquatable<IRS>
    {
        // notation => fixed = leg1;
        //          => float = leg2;


        // Keep fixed/float notation in IRS constructor;
        public IRS(DateTime Start, DateTime Maturity, string DayRule, string FloatDayCount, string FixedDayCount,
                   Index FloatIndex, Index FixedIndex, BusinessCalendar calendar) :
        base(Start, Maturity, DayRule, FloatDayCount, FixedDayCount, FloatIndex, FixedIndex, calendar)
        { }

        // Implement default constructor
        public IRS() : base() { }

        public override double Price(CurveModel model)
        {
            InitCheck(model.Anchor);

            double tmp_fixed = CurveModelHelper.Annuity(model, leg1_schedule, leg1_daycount);
            double tmp_float = 0;

            foreach (var row in leg2_schedule.dates)
            {
                tmp_float += row.Cvg(leg2_daycount) * model.DiscFactor(row.adjEnd, leg2_daycount) * model.Forward(leg2_index.getValue(), row.adjStart, row.adjEnd, leg2_daycount);
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
                tmp_float += row.Cvg(leg2_daycount) * model.DiscFactor(row.adjEnd, leg2_daycount) * model.Forward(leg2_index.getValue(), row.adjStart, row.adjEnd, leg2_daycount);
            }

            return tmp_float - fixed_rate * tmp_fixed;
        }

        public override double BackdatedNPV(CurveModel model, double fixed_rate, double current_fix)
        {
            InitCheck(model.Anchor);

            double tmp_fixed = CurveModelHelper.Annuity(model, leg1_schedule, leg1_daycount);
            double tmp_float = 0;

            foreach (var row in this.leg2_schedule.dates)
            {
                if (row.adjStart < model.Anchor)
                {
                    // This forward rate is already fixed;
                    tmp_float += row.Cvg(leg2_daycount) * model.DiscFactor(row.adjEnd, leg2_daycount) * current_fix;
                }
                else
                {
                    tmp_float += row.Cvg(leg2_daycount) * model.DiscFactor(row.adjEnd, leg2_daycount) * model.Forward(leg2_index.getValue(), row.adjStart, row.adjEnd, leg2_daycount);
                }
            }

            return tmp_float - fixed_rate * tmp_fixed;
        }


        public bool Equals(IRS o)
        {
            return this.Equals((Swap)o);
        }

    }

}
