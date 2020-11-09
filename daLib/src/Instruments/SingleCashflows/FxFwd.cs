using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.Currencies;
using daLib.DateUtils;
using daLib.Instruments;
using daLib.Model;
using System;


namespace daAnalytics.Instruments.SingleCashflows
{ 
    public class FxFwd : SingleCashflow
    {
        public Index counterIndex;
        public ccyPair pair;

        public FxFwd(DateTime Start, DateTime Maturity, Index baseIndex, Index counterIndex,ccyPair pair, string DayRule, string DayCount, BusinessCalendar calendar) : base(Start, Maturity, baseIndex, DayRule, DayCount, calendar)
        {

            this.counterIndex = counterIndex;
            this.pair = pair;
        }

        public override double NPV(CurveModel model, double fixed_rate)
        {
            throw new NotImplementedException();
        }

        public override double Price(CurveModel model)
        {
            throw new NotImplementedException();
        }

        protected override void HandleClone(Instrument clone) 
        {
            throw new NotImplementedException();
        }
    }
}
