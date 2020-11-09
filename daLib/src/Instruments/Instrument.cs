
using System;

using daLib.Conventions.Calenders;
using daLib.Model;
using daLib.DateUtils;

namespace daLib.Instruments
{
    // Instrument leafs    
    public abstract class Instrument : IEquatable<Instrument>, IDeepClone
    {
        public DateTime unadjEnd;
        public DateTime unadjStart;
        public string DayRule;
        public BusinessCalendar calendar;

        public string dateString; // Bit of a hack -> mb make function to get from DateTime to DateStrings;
        protected Instrument()
        {
            // Default value - This is known when combined with an curve i.e. an anchor
            dateString = null;
        }
        public virtual bool Equals(Instrument o)
        {
            return new Tuple<DateTime, DateTime, string>(this.unadjStart, this.unadjEnd, this.DayRule) == new Tuple<DateTime, DateTime, string>(o.unadjStart, o.unadjEnd, o.DayRule);
        }
        public virtual void SaveDateStrings(string dateString)
        {
            this.dateString = dateString;
        }
        public virtual object DeepClone()
        {
            return this;
            /*
            Instrument clone = (Instrument)this.MemberwiseClone();
            HandleClone(clone);
            return clone;
            */
        }
        public virtual DateTime adjStart()
        {
            return DateTimeUtils.AdjustDate(unadjStart,calendar,this.DayRule);
        }
        public virtual DateTime adjEnd()
        {
            return DateTimeUtils.AdjustDate(unadjEnd,calendar,this.DayRule);
        }
        public virtual string DateToString(DateTime d)
        {
            return d.ToString("ddMMMyyyy");
        }

        public virtual void RemoveTempObjects()
        {
            
        }
        protected abstract void HandleClone(Instrument clone);
        public abstract double Price(CurveModel model);
        public abstract double NPV(CurveModel model, double fixed_rate);
        public virtual double BackdatedNPV(CurveModel model, double fixed_rate, double last_fix)
        {
            return this.NPV(model, fixed_rate);
        }

    }



    
   
   

}







