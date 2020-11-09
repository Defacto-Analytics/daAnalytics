
using System;

using daLib.Conventions;
using daLib.Model;
using daLib.Instruments;

namespace daLib.Blocks
{

    public class BuildingBlock : IEquatable<BuildingBlock>, IDeepClone<BuildingBlock>
    {
        public CalibrationInstrument calibrationinstrument;
        public double marketQuote;
        public Instrument instrument;

        public BuildingBlock(Instrument instrument, double marketQuote)
        {
            this.instrument = instrument;
            this.marketQuote = marketQuote;
        }


        public BuildingBlock(Instrument instrument, double marketQuote, CalibrationInstrument calibrationinstrument)
        {
            this.calibrationinstrument = calibrationinstrument;
            this.instrument = instrument;
            this.marketQuote = marketQuote;
        }

        public bool Equals(BuildingBlock o)
        {
            return (this.marketQuote == o.marketQuote && instrument.Equals(o.instrument));
        }

        public double Price(CurveModel model)
        {
            return instrument.Price(model);
        }

        public override string ToString()
        {
            return instrument.ToString();
        }

        public BuildingBlock DeepClone()
        {
            return new BuildingBlock((Instrument)this.instrument.DeepClone(), this.marketQuote);
        }

        object IDeepClone.DeepClone()
        {
            return this.DeepClone();
        }
    }

    public class PortfolioBlock : ICloneable
    {
        public double tradedPrice;
        public double Notional;
        public double? lastFix;
        public Instrument instrument;
        public string blockID;
        public Index index;


        public PortfolioBlock(string ID, Instrument instrument, double Notional, double tradedPrice)
        {
            this.tradedPrice = tradedPrice;
            this.instrument = instrument;
            this.Notional = Notional;
            this.blockID = ID;
        }

        public PortfolioBlock(string ID, DateTime start, DateTime end, Index index, double notional, double tradedPrice)
        {
            this.instrument = InstrumentBuilder.BuildSwapFromConvention(start, end, index);
            this.index = index;
            this.Notional = notional;
            this.tradedPrice = tradedPrice;
            this.blockID = ID;
        }


        public PortfolioBlock(string ID, DateTime start, DateTime end, Index index, double notional, double tradedPrice, double lastFix)
        {
            this.instrument = InstrumentBuilder.BuildSwapFromConvention(start, end, index);
            this.index = index;
            this.Notional = notional;
            this.tradedPrice = tradedPrice;
            this.blockID = ID;
            this.lastFix = lastFix;
        }
       
        // Overload this to create instrument on the fly

        public double Price()
        {
            return this.tradedPrice;
        }

        public double NPV(CurveModel model)
        {
            if (lastFix.HasValue)
            {
                return this.instrument.BackdatedNPV(model, this.tradedPrice, this.lastFix.Value)*Notional;
            }
            else
            {
                return this.instrument.NPV(model, this.tradedPrice) * Notional;
            }
        }

        public double NPV(CurveModel model, double bumpedTradedPrice)
        {
            if (lastFix.HasValue)
            {
                return this.instrument.BackdatedNPV(model, bumpedTradedPrice, this.lastFix.Value) * Notional;
            }
            else
            {
                return this.instrument.NPV(model, bumpedTradedPrice) * Notional;
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
