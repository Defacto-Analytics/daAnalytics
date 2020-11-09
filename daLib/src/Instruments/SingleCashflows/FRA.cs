using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.Exceptions;
using daLib.Instruments;
using daLib.Model;
using System;


namespace daAnalytics.Instruments.SingleCashflows
{
    public class FRA : SingleCashflow
    {
       // DateTime TradeDate; // Anchor date - there should be a difference but we are saying they are the same right now.
       // DateTime SpotDate; // we are basically ignoring spotdates;
       // DateTime Fixing; // Start in our lingp 
       // DateTime StartAccr; // 
       // DateTime EndAccr;
       // DateTime EndFixing;


        public FRA(DateTime Start, DateTime Maturity, Index index, string DayRule, string DayCount, BusinessCalendar calendar) : base(Start, Maturity, index, DayRule, DayCount, calendar)
        {
        }

        public override double NPV(CurveModel model, double fixed_rate)
        {
            if (!model.isIndexInModel(baseIndex))
            {
                throw new ExcelException($"Model isn't calibrated to handle the index: {baseIndex.getValue()}");
            }
            return model.Forward(baseIndex.getValue(), this.adjStart(), this.adjEnd(), this.DayCount) - fixed_rate;
        }

        public override double Price(CurveModel model)
        {
            if (!model.isIndexInModel(baseIndex))
            {
                throw new ExcelException($"Model isn't calibrated to handle the index: {baseIndex.getValue()}");
            }
            return model.Forward(baseIndex.getValue(), this.adjStart(), this.adjEnd(), this.DayCount);
        }

        protected override void HandleClone(Instrument clone)
        {
        }
    }
}
