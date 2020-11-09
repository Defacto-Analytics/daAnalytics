
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


using daLib.Blocks;
using daLib.Conventions;
using daLib.DateUtils;
using daLib.Exceptions;
using daLib.Instruments;
using daLib.Math.Optimization;


namespace daLib.Model
{
    public class CurveModel : IDeepClone<CurveModel>
    {
        // Fundamental change to CurveModel;
        //      All Curves are inside ForwardCurves -> DiscCurve is just a reference to one of the curves inside ForwardCurves;
        //      This is very important to remember - because then it makes sense that we only calibrate on the forwardcurves.
        //          

        public DateTime Anchor;
        public string CurveModel_ID;
        public Curve DiscCurve; // Essentially just an reference to a curve in ForwardCurves Dictionary;
        public Dictionary<string, Curve> ForwardCurves;

        public DateTime? LastValidInput;
        public DateTime? LastCalibrated;
        public string LongestCurve;
        public int lenLongestCurve;


        public string calibration_id; // Need a global id - should also be the key to the disc curve from forwardcurves

        public CurveModel(DateTime Anchor, string CurveModel_ID)
        {
            this.Anchor = Anchor;
            this.CurveModel_ID = CurveModel_ID;
            EmptyInit();
        }

        public void ClearCurveModel()
        {
            EmptyInit();
        }

        public void ClearBuildingBlocks()
        {
            foreach (KeyValuePair<string,Curve> kvp in ForwardCurves)
            {
                kvp.Value.ClearBuildingBlocks();
            }
        }

        private void EmptyInit()
        {
            this.ForwardCurves = new Dictionary<string, Curve>();
            this.DiscCurve = null;
            this.LastCalibrated = null;
            LongestCurve = null;
            lenLongestCurve = 0;
            calibration_id = null;
        }

        public void addCurve(Index index)
        {
            ForwardCurves.Add(index.getValue() ,new Curve(index));
        }

        public int addInputQuote(string CalibrationInstrument, double quote)
        {
            // indicator 0 not in model at all;
            // indicator 1 in model;
            // indicator 2 in model, but different quote;

            CalibrationInstrument _CalibrationInstrument = new CalibrationInstrument(CalibrationInstrument, quote);
            int indicator = CheckBuildingBlocksInstruments(_CalibrationInstrument);
            if (indicator == 1)
            {
                return indicator;
            }

            Instrument instrument = InstrumentBuilder.GetInstrumentFromParsedString(this.Anchor,_CalibrationInstrument); instrument.SaveDateStrings(_CalibrationInstrument._strStart+ " " + _CalibrationInstrument._strTenor); ;
            BuildingBlock buildingblock = new BuildingBlock(instrument, quote, _CalibrationInstrument);
            ConventionLayer.Validate(_CalibrationInstrument._index);
            // Only populate forward curve -> choose which curve to discount with in calibration method

            if (this.ForwardCurves.ContainsKey(_CalibrationInstrument._index.getValue()))
            {
                if (indicator == 0)
                {
                    this.ForwardCurves[_CalibrationInstrument._index.getValue()].addBuildingBlock(this.Anchor, buildingblock);
                }
                else if (indicator == 2)
                {
                    this.ForwardCurves[_CalibrationInstrument._index.getValue()].replaceBuildingBlock(buildingblock);
                }
            }
            else
            {
                this.addCurve(_CalibrationInstrument._index);
                this.ForwardCurves[_CalibrationInstrument._index.getValue()].addBuildingBlock(this.Anchor, buildingblock);
            }
            return indicator;
        }

        public double DiscFactor(DateTime target, string DayCount)
        {
            // assume curve is calibrated and therefore disccurve defined.
            return DiscCurve.DiscFactor(this.Anchor,target, DayCount);
        }

        public double Forward(string IndexCurveID, DateTime Start, DateTime End, string DayCount)
        {
            Curve ForwardCurve;
            try
            {
                ForwardCurve = this.ForwardCurves[IndexCurveID];
            }
            catch (Exception)
            {
                throw new ExcelException($"Can't find {IndexCurveID.ToUpper()} in model {this.CurveModel_ID.ToUpper()}");
            }

            return (ForwardCurve.DiscFactor(Anchor, Start, DayCount) / ForwardCurve.DiscFactor(Anchor, End, DayCount) - 1.0) / DateTimeUtils.Cvg(Start, End, DayCount);
        }
        
        private Tuple<string[], Tuple<int, int>[], BuildingBlock[], double[]> StackCurves()
        {

            List<string> CurveOrder = new List<string>();
            List<double> StackedCurves = new List<double>();
            List<Tuple<int,int>> CurveIndex = new List<Tuple<int,int>>();
            List<BuildingBlock> StackedBuildingBlocks = new List<BuildingBlock>();


            int n, m, x, len;

            n = 0;
            m = 0;
            x = 0;
            len = ForwardCurves.Count;
            foreach (KeyValuePair<string, Curve> pair in ForwardCurves)
            {
                // Counting for last curve
                x++;

                CurveOrder.Add(pair.Key);
                StackedBuildingBlocks.AddRange(pair.Value.BuildingBlocks);

                foreach (Point p in pair.Value.zeroRates)
                {
                    StackedCurves.Add(p.y);
                    n++;
                }
                // Zero indexed adjustment
                if(x == 1)
                {
                    n--;
                }


                CurveIndex.Add(new Tuple<int, int>(m,n));

                m = n;
                m++;
            }

            return new Tuple<string[], Tuple<int, int>[], BuildingBlock[], double[]>(CurveOrder.ToArray(), CurveIndex.ToArray(), StackedBuildingBlocks.ToArray() ,StackedCurves.ToArray());
        }


        public bool isIndexInModel(Index idx)
        {
            foreach (KeyValuePair<string, Curve> pair in this.ForwardCurves)
            {
                if (pair.Key == idx.getValue())
                {
                    return true;
                }
            }
            return false;
        }


        // indicator 0 not in model at all;
        // indicator 1 in model;
        // indicator 2 in model, but different quote;
        // maybe change to enum
        public int CheckBuildingBlocksInstruments(CalibrationInstrument ci)
        {
            int indicator = 0;
            foreach (KeyValuePair<string,Curve> kvp in this.ForwardCurves)
            {
                foreach (BuildingBlock block in kvp.Value.BuildingBlocks)
                {
                    if (block.calibrationinstrument.InstrumentEqual(ci))
                    {
                        indicator++;
                    }
                    else
                    {
                        continue;
                    }

                    if (block.calibrationinstrument.QuotetEqual(ci))
                    {
                        indicator++;
                        return indicator;
                    }
                }
            }
            return indicator;
        }


        public void Calibrate(Index idx)
        {
            updateModelInfo();
            if (!this.isIndexInModel(idx))
            {
                throw new ExcelException("Index not compatible with calibration instruments");
            }


            calibration_id = idx.getValue();
            DiscCurve = ForwardCurves[calibration_id];

            daLib.Math.Vector obsX = new daLib.Math.Vector(DiscCurve.BuildingBlocks.Select(block => DateTimeUtils.DateTimeToSerial(block.instrument.adjEnd())).ToArray());
            daLib.Math.Vector obsY = new daLib.Math.Vector(DiscCurve.BuildingBlocks.Select(block => block.marketQuote).ToArray());

            RepricingFunction costFunction = new RepricingFunction(obsX, obsY, this.DiscCurve,this);
            daLib.Math.Optimization.Constraint constraint = new BoundaryConstraint(-1, 1);

            Problem problem = new Problem(costFunction, constraint, obsY);
            EndCriteria endCriteria = new EndCriteria(50000, null, 1E-15, 1E-15, null);

            LevenbergMarquardt optMethod = new LevenbergMarquardt(1E-15, 1E-15, 1E-15);
            var crit = optMethod.minimize(problem, endCriteria);
 

            foreach (KeyValuePair<string, Curve> entry in ForwardCurves)
            {
                if (entry.Key == calibration_id)
                {
                    // Dont recalibrate on the chosen disc curve
                    continue;
                }

                obsX = new daLib.Math.Vector(entry.Value.BuildingBlocks.Select(block => DateTimeUtils.DateTimeToSerial(block.instrument.adjEnd())).ToArray());
                obsY = new daLib.Math.Vector(entry.Value.BuildingBlocks.Select(block => block.marketQuote).ToArray());
                costFunction.update(obsX,obsY,entry.Value);
                problem = new Problem(costFunction, constraint, obsY);

                crit = optMethod.minimize(problem, endCriteria);
            }

            LastCalibrated = DateTime.Now;
        }

        public void Calibrate(Curve DiscCurve)
        {
            daLib.Math.Vector obsX = new daLib.Math.Vector(DiscCurve.BuildingBlocks.Select(block => DateTimeUtils.DateTimeToSerial(block.instrument.adjEnd())).ToArray());
            daLib.Math.Vector obsY = new daLib.Math.Vector(DiscCurve.BuildingBlocks.Select(block => block.marketQuote).ToArray());

            RepricingFunction costFunction = new RepricingFunction(obsX, obsY, this.DiscCurve, this);
            daLib.Math.Optimization.Constraint constraint = new BoundaryConstraint(-1, 1);

            Problem problem = new Problem(costFunction, constraint, obsY);
            EndCriteria endCriteria = new EndCriteria(50000, null, 1E-15, 1E-15, null);

            LevenbergMarquardt optMethod = new LevenbergMarquardt(1E-15, 1E-15, 1E-15);
            var crit = optMethod.minimize(problem, endCriteria);


            foreach (KeyValuePair<string, Curve> entry in ForwardCurves)
            {
                if (entry.Key == calibration_id)
                {
                    // Dont recalibrate on the chosen disc curve
                    continue;
                }

                obsX = new daLib.Math.Vector(entry.Value.BuildingBlocks.Select(block => DateTimeUtils.DateTimeToSerial(block.instrument.adjEnd())).ToArray());
                obsY = new daLib.Math.Vector(entry.Value.BuildingBlocks.Select(block => block.marketQuote).ToArray());
                costFunction.update(obsX, obsY, entry.Value);
                problem = new Problem(costFunction, constraint, obsY);

                crit = optMethod.minimize(problem, endCriteria);
            }

            LastCalibrated = DateTime.Now;
        }

        public double[,] EstimateJacobian()
        {
            CurveModel tmpModel = this.DeepClone();
            Tuple<string[], Tuple<int,int>[], BuildingBlock[], double[]> stack = tmpModel.StackCurves();
            // 

            double[] fvec = new double[stack.Item3.Length]; // price of buildingblocks
            double[] x = stack.Item4; // zero rates


            for (int i = 0; i < fvec.Length; i++)
            {
                fvec[i] = stack.Item3[i].Price(this);
            }

            Func<double[], double[]> func = (_x) =>
             {
                 // Map everything out - and save on tmpmodel
                 // save prices
                 Curve c;
                 double[] tmp;
                 int n, m;
                 double[] res = new double[_x.Length];
                 for (int i = 0; i < stack.Item1.Length; i++)
                 {
                     c = tmpModel.ForwardCurves[stack.Item1[i]];
                     m = stack.Item2[i].Item1;
                     n = stack.Item2[i].Item2;
                     tmp = new double[n - m + 1];
                     Array.Copy(_x, m, tmp, 0, n - m + 1);
                     c.addMultiplePointsAndReplace(tmp);
                 }

                 for (int i = 0; i < _x.Length; i++)
                 {
                     res[i] = stack.Item3[i].Price(tmpModel);
                 }

                 return res;
             };

            return NumericalForwardJacobian(x, fvec, func);

        }

        public double[,] NumericalForwardJacobian(double[] x, double[] fvec, Func<double[],double[]> func)
        {
            const double eps = 1.0E-8;
            int n = x.Length;
            double[,] df = new double[n, n];
            double[] xh = x;

            for (int i = 0; i < n; i++)
            {
                double temp = xh[i];
                double h = eps * System.Math.Abs(temp);
                if (h == 0.0)
                {
                    h = eps;
                }
                xh[i] = temp + h;
                h = xh[i] - temp;

                double[] f = func(xh);
                xh[i] = temp;


                for (int j = 0; j < n; j++)
                {
                    df[j, i] = (f[j] - fvec[j]) / h;
                }
            }

            return df;
            
        }

        public CurveModel DeepClone()
        {
            CurveModel newCurveModel = this.MemberwiseClone() as CurveModel;
            newCurveModel.ForwardCurves = new Dictionary<string, Curve>();

            foreach (KeyValuePair<string,Curve> a in this.ForwardCurves)
            {
                newCurveModel.ForwardCurves[a.Key] = a.Value.DeepClone();
            }

            if (LastCalibrated == null)
            {
                newCurveModel.DiscCurve = null;
            }
            else
            {
                newCurveModel.DiscCurve = newCurveModel.ForwardCurves[calibration_id];
            }

            return newCurveModel;

        }

        object IDeepClone.DeepClone()
        {
            return this.DeepClone();
        }
        public void updateModelInfo()
        {
            int n = 0;
            string tmp = "";
            foreach (KeyValuePair<string, Curve> pair in this.ForwardCurves)
            {
                if (n < pair.Value.BuildingBlocks.Count)
                {
                    n = pair.Value.BuildingBlocks.Count;
                    tmp = pair.Key;
                }
            }

            this.LongestCurve = tmp;
            this.lenLongestCurve = n;
        }

        public void calibrationCleanup()
        {
            foreach (KeyValuePair<string,Curve> kvp in ForwardCurves)
            {
                foreach (BuildingBlock block in kvp.Value.BuildingBlocks)
                {
                    block.instrument.RemoveTempObjects();
                }
            }
        }

#region private helpers

        private double CalibrationAccuracyLeastSquares()
        {
            double sum = 0;
            foreach (BuildingBlock b in DiscCurve.BuildingBlocks)
            {
                sum += System.Math.Pow(b.Price(this) - b.marketQuote, 2);
            }

            foreach (KeyValuePair<string, Curve> item in ForwardCurves)
            {
                foreach (BuildingBlock b in item.Value.BuildingBlocks)
                {
                    sum += System.Math.Pow(b.Price(this) - b.marketQuote, 2);
                }
            }

            return sum;
        }
#endregion
    }

    class RepricingFunction : CostFunction
    {
        private daLib.Math.Vector x_;
        private daLib.Math.Vector target_;
        private Curve c_;
        private CurveModel model_;

        public RepricingFunction(daLib.Math.Vector x, daLib.Math.Vector target, Curve c, CurveModel model)
        {
            x_ = new daLib.Math.Vector(x);
            target_ = new daLib.Math.Vector(target);
            c_ = c;
            model_ = model;
        }

        public void update(daLib.Math.Vector x, daLib.Math.Vector target, Curve c)
        {
            x_ = new daLib.Math.Vector(x);
            target_ = new daLib.Math.Vector(target);
            c_ = c;
        }

        public override daLib.Math.Vector values(daLib.Math.Vector p)
        {
            c_.addMultiplePointsAndReplace(x_.ToArray(),p.ToArray());
            daLib.Math.Vector y = new daLib.Math.Vector(p.size());

            for (int i = 0; i < y.Count; i++)
            {
                y[i] = c_.BuildingBlocks[i].Price(model_) - target_[i];
            }

            return y;
        }
    }
}


