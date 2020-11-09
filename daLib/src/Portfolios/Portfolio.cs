using System.Collections.Generic;
using System.Linq;
using daLib.Blocks;
using daLib.Model;


namespace daLib.Portfolios
{
    interface IPortfolio
    {
        double NPV(CurveModel model);
        Dictionary<string,double[]> ModelRiskDictionary(CurveModel model, double bumpBP = 1);
        double DVO1(CurveModel model, double bumpBP = 1);
    }

    public class Portfolio : IPortfolio
    {
        public double _NPV;
        public List<PortfolioBlock> pf;
        public string Portfolio_ID;


        public Portfolio(CurveModel model)
        {
            this._NPV = this.NPV(model);
        }

        public Portfolio(string Portfolio_ID)
        {
            this.Portfolio_ID = Portfolio_ID;
            EmptyInit();
        }

        public void ClearPortfolio()
        {
            EmptyInit();
        }

        private void EmptyInit()
        {
            this.pf = new List<PortfolioBlock>();
        }

        public void RemovePortfolioBlock(string[] ID)
        {

            List<PortfolioBlock> tmp = new List<PortfolioBlock>();

            foreach (PortfolioBlock block in pf)
            {
                if (Helper.StringExistsInArray(ID,block.blockID))
                {
                    tmp.Add(block);
                }
            }

            foreach (PortfolioBlock block in tmp)
            {
                this.pf.Remove(block);
            }
            
        }

        public void addBlock(PortfolioBlock b)
        {
            pf.Add(b);
        }

        public PortfolioBlock[] getBlocks(string[] BlockID)
        {
            PortfolioBlock[] res = pf.FindAll(item => BlockID.Contains(item.blockID)).ToArray();
            return res;
        }


        public double NPV(CurveModel model)
        {
            double NPV = 0;
            foreach (PortfolioBlock block in pf)
            {
                NPV += block.NPV(model);
            }
            return NPV;
        }

        public double DVO1(CurveModel model, double bumpBP = 1)
        {

            // remember to clone this - not just take the reference
            List<PortfolioBlock> bumped_pf = pf;

            double bumpedNPV = 0;
            foreach (PortfolioBlock block in bumped_pf)
            {
                block.tradedPrice += bumpBP / 10000;
                bumpedNPV += block.NPV(model, block.tradedPrice);
            }

            return bumpedNPV - this._NPV;   
        }

        // Bump zero rates one by one
        public Dictionary<string,double[]> ModelRiskDictionary(CurveModel model, double bumpBP = 1)
        {
            CurveModel tmpModel = model.DeepClone();
            List<double> holder = new List<double>();
            Dictionary<string, double[]> result = new Dictionary<string, double[]>();
            double sum;
            Point tmp;

            foreach (KeyValuePair<string,Curve> pair in tmpModel.ForwardCurves)
            {
                for (int i = 0; i < pair.Value.zeroRates.Count; i++)
                {
                    tmp = pair.Value.zeroRates[i]; // Struct so deep copying here

                    pair.Value.SetZeroRate(new Point(tmp.x, tmp.y + bumpBP / 10000.0), i);

                    sum = this.NPV(tmpModel) - this.NPV(model);

                    pair.Value.SetZeroRate(tmp, i);

                    holder.Add(sum);
                }

                result.Add(pair.Key, holder.ToArray());
                holder.Clear();
            }
            return result;
        }

        public double[] ModelRiskVector(CurveModel model)
        {

            var modelRiskDict = this.ModelRiskDictionary(model);
            
            int len = 0;
            foreach (var item in modelRiskDict)
            {
                len += item.Value.Length;
            }

            double[] result = new double[len];

            int count = 0;
            foreach (KeyValuePair<string, double[]> pair in modelRiskDict)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    result[count] = pair.Value[i];
                    count++;
                }
            }
            return result;
        }


        public double[,] UnstackRisk(CurveModel model, double[] stackedRisk)
        {

            int idx = 0;
            int j = 0;
            double[,] result = new double[model.lenLongestCurve, model.ForwardCurves.Count];
            foreach (KeyValuePair<string,Curve> kvp in model.ForwardCurves)
            {
                for (int i = 0; i < kvp.Value.BuildingBlocks.Count; i++)
                {
                    result[i, idx] = stackedRisk[j];
                    j++;
                }
                idx++;
            }

            return result;
        }


    }
}
