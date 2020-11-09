
using daLib;
using daLib.Blocks;
using daLib.Conventions;
using daLib.Conventions.Calenders;
using daLib.DateUtils;
using daLib.Exceptions;
using daLib.Instruments;
using daLib.Instruments.Swaps;
using daLib.Math;
using daLib.Model;
using daLib.Portfolios;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace daAnalyticsExcel.Exposure
{
    public partial class ExcelFunctionExposure : IExcelAddIn
    {

        #region OfficalRegion

        [ExcelFunction(Name = prefix + "CreateModel")]
        public static string CreateModel(DateTime Anchor, string CurveModel_ID)
        {
            CurveModel_ID = CurveModel_ID.ToLower();

            if (CurveSet.ContainsKey(CurveModel_ID))
            {
                return "Model already exists";
            }

            CurveSet.Add(CurveModel_ID, new CurveModel(Anchor, CurveModel_ID));

            return "Model Created";
        }

        [ExcelFunction(Name = prefix + "ClearModel")]
        public static string ClearModel(string CurveModel_ID)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            
            model.ClearCurveModel();
            return (CurveModel_ID + " is cleared");
        }

        [ExcelFunction(Name = prefix + "CreatePortfolio")]
        public static string CreatePortfolio(string Portfolio_ID)
        {
            Portfolio_ID = Portfolio_ID.ToLower();

            if (PortfolioSet.ContainsKey(Portfolio_ID))
            {
                return Portfolio_ID;
            }

            PortfolioSet.Add(Portfolio_ID, new Portfolio(Portfolio_ID));

            return Portfolio_ID;
        }



        [ExcelFunction(Name = prefix + "ClearPortfolio")]
        public static string ClearPortfolio(string Portfolio_ID)
        {
            // Input validation -- start
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);
            // Input validation -- end

            port.ClearPortfolio();
            return (Portfolio_ID + " is removed");
        }

        [ExcelFunction(Name = prefix + "Interpolate")]
        public static double Interpolate(double[] X, double[] Y, double x)
        {
            if (X.Length != Y.Length)
            {
                throw new ExcelException("X and Y needs to be same length");
            }

            Array.Sort(X, Y);
            CubicSpline inter = CubicSpline.BuildHermiteInterpolaterSorted(X, Y, InterpolationHelper.BesselFirstDerivatives(X, Y));
            return inter.Interpolate(x);
        }

        [ExcelFunction(IsVolatile = true, Name = prefix + "InputQuotes")]
        public static DateTime InputQuotes(string CurveModel_ID, object[] CalibrationInstruments, double[] InputQuotes)
        {
            // Input validation -- start
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            // Input validation -- end
            try
            {
                for (int i = 0; i < CalibrationInstruments.Length; i++)
                {
                    model.addInputQuote((string)CalibrationInstruments[i], InputQuotes[i]);
                }
                model.LastValidInput = DateTime.Now;
                return model.LastValidInput.Value;
            }
            catch (Exception)
            {
                throw new ExcelException("Non-valid input quotes given");
            }            
        }

       
        [ExcelFunction(Description = "Adds days specified by a tenor, dayrule and calendar.", Name = prefix + "AddTenor")]
        public static DateTime AddTenor(DateTime startdate, string tenor,
         [ExcelArgument(Description = "Optional: Will default to modified following")] object dayrule,
         [ExcelArgument(Description = "Optional: Will default to weekends only businesscalendar")] object calendar)
        {
            DayRule _dayrule = dayrule is ExcelMissing ? new DayRule("mf") : new DayRule(dayrule.ToString());
            BusinessCalendar _calendar = calendar is ExcelMissing ? new WeekendsOnly() : new BusinessCalendar(calendar.ToString());
            ConventionLayer.Validate(_dayrule, _calendar);
            return DateTimeUtils.AddTenor(startdate, tenor, _calendar, _dayrule.getValue());
        }

        [ExcelFunction(Name = prefix + "GetCalendars")]
        public static object[,] GetCalendars()
        {
            BusinessCalendar a = new Denmark();
            BusinessCalendar b = new TARGET2();
            BusinessCalendar c = new WeekendsOnly();

            object[] res = new object[] { a.name(), b.name(), c.name() };
            return Helper.Transpose(res);
        }

        [ExcelFunction(Name = prefix + "Calibrate")]
        public static string Calibrate(string CurveModel_ID, string Index)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            string result = CalibrateImplement(CurveModel_ID, Index);
            stopwatch.Stop();
            return $"{result} time (seconds): {stopwatch.Elapsed.TotalSeconds}";
        }

        private static string CalibrateImplement(string CurveModel_ID, string Index)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            Index idx = new Index(Index);
            model.Calibrate(idx);
            return "Model calibrated.";
        }

        [ExcelFunction(IsVolatile = true, Name = prefix + "LastCalibrated")]
        public static object LastCalibrated(string CurveModel_ID)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            if (model.LastCalibrated == null)
            {
                return "Not yet calibrated";
            }
            return model.LastCalibrated;
        }

        [ExcelFunction(IsVolatile = true, Name = prefix + "LastValidInputQuote")]
        public static object LastValidInputQuote(string CurveModel_ID)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            if (model.LastValidInput == null)
            {
                return "No valid inputs quotes";
            }
            return model.LastValidInput;
        }

        [ExcelFunction(Name = prefix + "SwapRate")]
        public static double SwapRate(string CurveModel_ID, string start, string end, string index)
        {
            // Input validation -- start
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            ValidDate validStart = new ValidDate(start, model.Anchor);
            ValidDate validEnd = new ValidDate(end, validStart.getValue());
            Index validIndex = new Index(index);
            ConventionLayer.Validate(validStart, validStart, validIndex);
            // Input validation -- end

            var Swap = InstrumentBuilder.BuildSwapFromConvention(validStart.getValue(), validEnd.getValue(), validIndex);
            return Swap.Price(model);
        }

        [ExcelFunction(Name = prefix + "SwapNPV")]
        public static double SwapNPV(string CurveModel_ID, string start, string end, string index, double rate)
        {
            // Input validation -- start
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            ValidDate validStart = new ValidDate(start, model.Anchor);
            ValidDate validEnd = new ValidDate(end, validStart.getValue());
            Index validIndex = new Index(index);
            ConventionLayer.Validate(validStart, validStart, validIndex);
            // Input validation -- end

            var Swap = InstrumentBuilder.BuildSwapFromConvention(validStart.getValue(), validEnd.getValue(), validIndex);
            return Swap.NPV(model, rate);
        }

        [ExcelFunction(Name = prefix + "Annuity")]
        public static double Annuity(string CurveModel_ID, string start, string end, string index)
        {

            // Input validation -- start
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            ValidDate validStart = new ValidDate(start, model.Anchor);
            ValidDate validEnd = new ValidDate(end, validStart.getValue());
            Index idx = new Index(index);
            DayCount daycount = new DayCount("act/360");
            DayRule dayrule = new DayRule("mf");
            BusinessCalendar calendar = new WeekendsOnly(); 
            ConventionLayer.Validate(validStart, validEnd, idx, daycount, dayrule);
            // Input validation -- end

            DateSchedule d = new DateSchedule(validStart.getValue(), validEnd.getValue(), calendar, null, dayrule.getValue());
            return CurveModelHelper.Annuity(model, d, daycount.getValue());
        }

        [ExcelFunction(Name = prefix + "ListModelsInMemory")]
        public static object[,] ListModelsInMemory()
        {
            object[] models;


            models = CurveSet.Keys.ToArray();

            if (models == null || models.Length == 0)
            {
                models = new object[] { "No Models in memory." };
            }

            object[,] modelsTransposed = new object[models.Length, 1];
            for (int i = 0; i < models.Length; i++)
            {
                modelsTransposed[i, 0] = models[i];
            }

            return modelsTransposed;

        }


        [ExcelFunction(Name = prefix + "ListPortfoliosInMemory")]
        public static object[,] ListPortfoliosInMemory()
        {
            object[] ports;


            ports = PortfolioSet.Keys.ToArray();

            if (ports == null || ports.Length == 0)
            {
                ports = new object[] { "No Portfolios in memory." };
            }

            object[,] portsTransposed = new object[ports.Length, 1];
            for (int i = 0; i < ports.Length; i++)
            {
                portsTransposed[i, 0] = ports[i];
            }

            return portsTransposed;

        }

        [ExcelFunction(Name = prefix + "Risk")]
        public static object[,] Risk(string Portfolio_ID, object[] CurveModel_ID)
        {
            if (CurveModel_ID.Length == 1)
            {
                return daRisk(Portfolio_ID, CurveModel_ID[0].ToString());
            }

            return daRiskModel(Portfolio_ID, CurveModel_ID);

        }

        private static object[,] daRisk(string Portfolio_ID, string CurveModel_ID)
        {
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);
            CurveModel model = TryGetCurveModel(CurveModel_ID);

            double[,] jacobian = model.EstimateJacobian();
            double[] modelRiskVector = port.ModelRiskVector(model);

            // curve order of this vector
            double[] stackedRisk = Helper.MatrixMultiply(Helper.MatrixInverse(Helper.MatrixTranspose(jacobian)), modelRiskVector);
            object[,] result = new object[stackedRisk.Length, 2];

            int idx = 0;
            foreach (KeyValuePair<string, Curve> item in model.ForwardCurves)
            {
                for (int i = 0; i < item.Value.BuildingBlocks.Count; i++)
                {
                    result[idx, 0] = item.Value.BuildingBlocks[i].instrument.ToString();
                    result[idx, 1] = stackedRisk[idx];
                    idx++;
                }
            }

            return result;

        }

        private static object[,] daRiskModel(string Portfolio_ID, object[] CurveModel_ID)
        {

            Portfolio dkkPort = new Portfolio("dkk");
            Portfolio eurPort = new Portfolio("eur");
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);

            foreach (PortfolioBlock block in port.pf)
            {
                if (block.index.currency == "dkk")
                {
                    dkkPort.addBlock(block);
                }
                else if (block.index.currency == "eur")
                {
                    eurPort.addBlock(block);
                }
                else
                {
                    throw new ExcelException("MAKES NO SENSE");
                }
            }


            CurveModel dkkCurve = new CurveModel(DateTime.Today, "outofscope1");
            CurveModel eurCurve = new CurveModel(DateTime.Today, "outofscope2");

            foreach (var item in CurveModel_ID)
            {
                foreach (var item2 in TryGetCurveModel(item.ToString()).ForwardCurves)
                {
                    if (item2.Value.Index.currency == "dkk")
                    {
                        dkkCurve = TryGetCurveModel(item.ToString());
                        break;
                    }
                    else if (item2.Value.Index.currency == "eur")
                    {
                        eurCurve = TryGetCurveModel(item.ToString());
                        break;
                    }
                    else
                    {
                        throw new ExcelException("YES, MI LORD - WORK WORK");
                    }
                }
                
            }

            object[,] dkkRisk = Helper.Risk(dkkPort, dkkCurve);
            object[,] eurRisk = Helper.Risk(eurPort, eurCurve);

            object[,] risk = new object[dkkRisk.GetUpperBound(0) + eurRisk.GetUpperBound(0) + 2, 2];

            int riskidx = 0;
            for (int i = 0; i < dkkRisk.GetUpperBound(0) + 1; i++)
            {
                risk[riskidx, 0] = dkkRisk[i, 0];
                risk[riskidx, 1] = dkkRisk[i, 1];
                riskidx++;
            }

            for (int i = 0; i < eurRisk.GetUpperBound(0) + 1; i++)
            {
                risk[riskidx, 0] = eurRisk[i, 0];
                risk[riskidx, 1] = eurRisk[i, 1];
                riskidx++;
            }

            return risk;
        }



        [ExcelFunction(Name = prefix + "AddPortfolioBlocks")]
        public static DateTime AddPortfolioBlocks(string Portfolio_ID, object[] ID, object[] start, object[] end, object[] index, object[] Notional, object[] TradedRates, object[] LastFix)
        {
          

            int[] arrLength = new int[7] { ID.Length, start.Length, end.Length, index.Length, Notional.Length, TradedRates.Length, LastFix.Length };
            if (!arrLength.All(element => element == ID.Length))
            {
                throw new ExcelException("Inputs needs to be same length");
            }

            double not_tmp;
            double traded_tmp;
            double lastfix_tmp;

            ValidDate validStart;
            ValidDate validEnd;
            Index validIndex;
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);
            PortfolioBlock block;

            for (int i = 0; i < ID.Length; i++)
            {

                
                if (ID[i].ToString() == "ExcelDna.Integration.ExcelEmpty" || ID[i].ToString() == "" || ID[i].ToString() == "ExcelDna.Integration.ExcelError")
                {
                    continue;
                }

                // Checks to see if the blockID already exists in the portfolio
                if (Helper.StringExistsInArray(port.pf.Select(pfBlock => pfBlock.blockID).ToArray(), ID[i].ToString()))
                {
                    continue;
                }

                validStart = new ValidDate(start[i].ToString());
                validEnd = new ValidDate(end[i].ToString(), validStart.getValue());
                validIndex = new Index(index[i].ToString());
                ConventionLayer.Validate(validStart, validEnd, validIndex);

                try
                {
                    not_tmp = Double.Parse(Notional[i].ToString());
                    traded_tmp = Double.Parse(TradedRates[i].ToString());

                }
                catch (Exception)
                {
                    throw new ExcelException("Can't convert notional or tradedrate to a number.");
                }


                if (LastFix[i].ToString() == "ExcelDna.Integration.ExcelEmpty" || LastFix[i].ToString() == "" || LastFix[i].ToString() == "ExcelDna.Integration.ExcelError")
                {
                    block = new PortfolioBlock(ID[i].ToString(), validStart.getValue(), validEnd.getValue(), validIndex, not_tmp, traded_tmp);
                }
                else
                {
                    try
                    {
                        lastfix_tmp = Double.Parse(LastFix[i].ToString());
                    }
                    catch (Exception)
                    {
                        throw new ExcelException("Can't convert LastFix to a number.");
                    }

                    block = new PortfolioBlock(ID[i].ToString(), validStart.getValue(), validEnd.getValue(), validIndex, not_tmp, traded_tmp, lastfix_tmp);
                }

                port.addBlock(block);
            }

            return DateTime.Now;
        }


        [ExcelFunction(Name = prefix + "GetPortfolioBlocks")]
        public static object[,] GetPortfolioBlocks(string Portfolio_ID, object[] CurveModel_ID)
        {
            if (CurveModel_ID.Length == 1)
            {
                return daGetPortfolioBlocks(Portfolio_ID, CurveModel_ID[0].ToString());
            }

            return daGetPortfolioBlocksMultiModel(Portfolio_ID, CurveModel_ID);

        }

        private static object[,] daGetPortfolioBlocks(string Portfolio_ID, string CurveModel_ID)
        {
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);
            CurveModel model = TryGetCurveModel(CurveModel_ID);

            object[,] result = new object[port.pf.Count + 1, 7];

            result[0, 0] = "ID";
            result[0, 1] = "Start";
            result[0, 2] = "End";
            result[0, 3] = "Index";
            result[0, 4] = "Notional";
            result[0, 5] = "TradedPrice";
            result[0, 6] = "NPV";

            int idx = 1;
            foreach (PortfolioBlock block in port.pf)
            {
                result[idx, 0] = block.blockID;
                result[idx, 1] = block.instrument.unadjStart;
                result[idx, 2] = block.instrument.unadjEnd;
                result[idx, 3] = block.index.getValue().ToUpper();
                result[idx, 4] = block.Notional;
                result[idx, 5] = block.tradedPrice;
                result[idx, 6] = block.NPV(model);
                idx++;
            }


            return result;
        }

        private static object[,] daGetPortfolioBlocksMultiModel(string Portfolio_ID, object[] CurveModel_ID)
        {
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);

            Dictionary<string, CurveModel> dictmodels = new Dictionary<string, CurveModel>();

            foreach (string item in CurveModel_ID)
            {

                try
                {
                    dictmodels.Add(item.ToString(), TryGetCurveModel(item.ToString()));
                }
                catch
                {
                    continue;
                }
            }

           
            object[,] result = new object[port.pf.Count + 1, 7];

            result[0, 0] = "ID";
            result[0, 1] = "Start";
            result[0, 2] = "End";
            result[0, 3] = "Index";
            result[0, 4] = "Notional";
            result[0, 5] = "TradedPrice";
            result[0, 6] = "NPV";

            int idx = 1;
            foreach (PortfolioBlock block in port.pf)
            {
                result[idx, 0] = block.blockID;
                result[idx, 1] = block.instrument.unadjStart;
                result[idx, 2] = block.instrument.unadjEnd;
                result[idx, 3] = block.index.getValue().ToUpper();
                result[idx, 4] = block.Notional;
                result[idx, 5] = block.tradedPrice;

                // pick first with correct index

                foreach (KeyValuePair<string,CurveModel> kvp in dictmodels)
                {
                    if (kvp.Value.isIndexInModel(block.index))
                    {
                        result[idx, 6] = block.NPV(kvp.Value);
                        break;
                    }
                }
                idx++;
            }


            return result;
        }

        #endregion

        #region IncludeWhenDebug
#if DEBUG

        [ExcelFunction(Name = prefix + "CalibrateAsync")]
        public static object CalibrateAsync(string CurveModel_ID, string Index)
        {
            var result = ExcelAsyncUtil.Run("CalibrateImplement", new[] { CurveModel_ID }, () => Calibrate(CurveModel_ID,Index));
            if (result.Equals(ExcelError.ExcelErrorNA))
            {
                return "### Executing ###";
            }

            
            return result;
        }

        [ExcelFunction(Name = prefix + "getInputQuotes")]
        public static object[,] getInputQuotes(string CurveModel_ID)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            List<BuildingBlock> list = new List<BuildingBlock>();


            // DiscCurve exists as a forwardcurve - the this.DiscCurve is just a reference to the same object
            foreach (KeyValuePair<string, Curve> pair in model.ForwardCurves)
            {
                list.AddRange(pair.Value.BuildingBlocks);
            }

            object[,] res = new object[list.Count, 2];
            for (int i = 0; i < list.Count; i++)
            {
                res[i, 0] = list[i].ToString();
                res[i, 1] = list[i].marketQuote;
            }

            return res;
        }

        [ExcelFunction(Name = prefix + "ModelRiskVector")]
        public static double[] ModelRiskVector(string Portfolio_ID, string CurveModel_ID)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);
            return port.ModelRiskVector(model);
        }

        [ExcelFunction(Name = prefix + "Jacobian")]
        public static double[,] Jacobian(string CurveModel_ID)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);

            return model.EstimateJacobian();
        }

        [ExcelFunction(Name = prefix + "Quote")]
        public static double Quote(string CurveModel_ID, string CalibrationInstrument)
        {

            CurveModel model = TryGetCurveModel(CurveModel_ID);
            var parsedInstrument = Helper.ParseInstrument(CalibrationInstrument, model.Anchor);
            var instrument = InstrumentBuilder.GetInstrumentFromParsedString(model.Anchor, new daLib.CalibrationInstrument(CalibrationInstrument));

            return instrument.Price(model);
        }

        [ExcelFunction(Name = prefix + "CountInputs")]
        public static string CountInputs(string CurveModel_ID)
        {
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            int zerorates = 0;
            int buildingblocks = 0;

            foreach (KeyValuePair<string,Curve> kvp in model.ForwardCurves)
            {
                zerorates += kvp.Value.zeroRates.Count;
                buildingblocks += kvp.Value.BuildingBlocks.Count;
            }

            return $"#zerorates = {zerorates}; #buildingblocks = {buildingblocks}";
            
        }


        [ExcelFunction(Name = prefix + "RemoveModel")]
        public static string RemoveModel(string CurveModel_ID)
        {
            // Input validation -- start
            CurveModel model = TryGetCurveModel(CurveModel_ID);
            // Input validation -- end


            CurveSet.Remove(CurveModel_ID.ToLower());
            return (CurveModel_ID + " is removed");
        }

        [ExcelFunction(Name = prefix + "RemovePortfolio")]
        public static string RemovePortfolio(string Portfolio_ID)
        {
            // Input validation -- start
            Portfolio port = TryGetPortfolioSet(Portfolio_ID);
            // Input validation -- end

            PortfolioSet.Remove(Portfolio_ID.ToLower());
            return (Portfolio_ID + " is removed");
        }

#endif
        #endregion

    }
}


