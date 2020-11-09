
using System;
using System.Collections.Generic;

using daLib.Blocks;
using daLib.Conventions;
using daLib.DateUtils;
using daLib.Exceptions;
using daLib.Math;

namespace daLib.Model
{

public class Curve : IDeepClone<Curve>
    {

        public List<Point> zeroRates;
        public List<BuildingBlock> BuildingBlocks;
        public CubicSpline interpolater;
        public Index Index; // Which index is this curve trying to pin down
        


        private int neededPoints;

        public Curve(Index index)
        {
            zeroRates = new List<Point>();
            BuildingBlocks = new List<BuildingBlock>();
            neededPoints = 5;
            this.Index = index;
        }

        public void UpdateInter()
        {
            if (this.neededPoints <= this.zeroRates.Count)
            {
                this.zeroRates.Sort();
                this.interpolater = CubicSpline.BuildHermiteInterpolaterSorted(Helper.xToArray(zeroRates), Helper.yToArray(zeroRates), InterpolationHelper.BesselFirstDerivatives(Helper.xToArray(zeroRates), Helper.yToArray(zeroRates)));
            }
        }

        public void SetZeroRate(Point p, int index)
        {
            this.zeroRates[index] = new Point(p.x, p.y);
            UpdateInter();
        }

        public double getRate(double x)
        {
            return this.interpolater.Interpolate(x);
        }

        public double getRate(DateTime Anchor, DateTime x)
        {
            return this.interpolater.Interpolate(DateTimeUtils.DateTimeToSerial(x));
        }

        public double DiscFactor(DateTime Anchor, DateTime target, string DayCount)
        {
            return System.Math.Exp(-getRate(Anchor, target) * DateTimeUtils.Cvg(Anchor, target, DayCount));
        }

        public void addPoint(double x, double y, bool updateinter = false)
        {
            this.zeroRates.Add(new Point(x, y));

            if (updateinter)
                this.UpdateInter();
        }

        public void addMultiplePoints(double[] x, double [] y)
        {
            if (x.Length != y.Length)
            {
                throw new ExcelException("x and y must be same length"); 
            }

            for (int i = 0; i < x.Length; i++)
            {
                this.addPoint(x[i], y[i]);
            }

            this.UpdateInter();
        }

        public void addMultiplePoints(double[] y)
        {
            if (zeroRates.Count != y.Length)
            {
                throw new ExcelException("x and y must be same length");
            }

            for (int i = 0; i < y.Length; i++)
            {
                this.zeroRates[i] = new Point(this.zeroRates[i].x, y[i]);
            }

            this.UpdateInter();
        }

        public void addMultiplePointsAndReplace(double[] x, double[] y)
        {
            this.zeroRates.Clear();
            this.addMultiplePoints(x, y);
        }

        public void addMultiplePointsAndReplace(double[] y)
        {
            this.addMultiplePoints(y);
        }

        public void addBuildingBlock(DateTime Anchor, BuildingBlock b)
        {
            this.addPoint(DateTimeUtils.DateTimeToSerial(b.instrument.adjEnd()), b.marketQuote, updateinter: true);
            this.BuildingBlocks.Add(b);
        }

        public void replaceBuildingBlock(BuildingBlock b)
        {
            for (int i = 0; i < this.BuildingBlocks.Count; i++)
            {
                if (this.BuildingBlocks[i].ToString() == b.calibrationinstrument.ToString())
                {
                    this.BuildingBlocks[i] = b;
                    return;
                }
            }
        }

        public void ClearBuildingBlocks()
        {
            BuildingBlocks.Clear();
        }

        public Curve DeepClone()
        {
            Curve newCurve = this.MemberwiseClone() as Curve;
            newCurve.BuildingBlocks = Helper.DeepCopyListOfRef<BuildingBlock>(this.BuildingBlocks);
            newCurve.zeroRates = Helper.DeepCopyListOfValue<Point>(this.zeroRates);
            newCurve.interpolater = this.interpolater.DeepClone();
            return newCurve;
        } 

        object IDeepClone.DeepClone()
        {
            return this.DeepClone();
        }
    }


}





