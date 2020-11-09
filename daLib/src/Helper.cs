using System;
using System.Collections.Generic;
using System.Linq;

using daLib.Conventions.Calenders;
using daLib.Model;
using daLib.DateUtils;
using daLib.Math;
using daLib.Portfolios;

namespace daLib
{
    public static class Helper
    {
        public static double GetRandomNumber(Random r, double minimum, double maximum)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }
        public static double[,] MatrixMultiply(double[,] A, double[,] B)
        {

            Matrix matA = new Matrix(A);
            Matrix matB = new Matrix(B);

            return (matA * matB).ToArray();
        }

        public static double[] MatrixMultiply(double[,] A, double[] B)
        {
            Matrix matA = new Matrix(A);
            Vector vecB = new Vector(B);

            return (matA * vecB).ToArray();
        }
        public static double[,] MatrixInverse(double[,] mat)
        {
            Matrix matA = new Matrix(mat);
            return Matrix.inverse(matA).ToArray();
        }

        public static double[,] MatrixTranspose(double[,] mat)
        {
            Matrix matA = new Matrix(mat);
            return Matrix.transpose(matA).ToArray();
        }



        public static bool StringExistsInArray(string[] arr, string str)
        {
            return arr.Contains(str);
        }


        public static T[] DeepCopyArrayOfValue<T>(T[] original)
        {
            T[] res = new T[original.Length];
            original.CopyTo(res, 0);
            return res;
        }

        public static T[] DeepCopyArrayOfRef<T>(T[] original) where T : IDeepClone<T>
        {
            T[] res = new T[original.Length];
            for (int i = 0; i < original.Length; i++)
            {
                res[i] = original[i].DeepClone();
            }
            return res;
        }

        public static List<T> DeepCopyListOfValue<T>(List<T> original)
        {
            List<T> res = new List<T>();
            foreach (T item in original)
            {
                res.Add(item);
            }
            return res;
        }

        public static List<T> DeepCopyListOfRef<T>(List<T> original) where T : IDeepClone<T>
        {
            List<T> res = new List<T>();
            foreach (T item in original)
            {
                res.Add(item.DeepClone());
            }
            return res;
        }

        public static void tmpInsertColumn(double[,] holder, double[] tmp, int idx)
        {
            int n = tmp.Length;

            for (int i = 0; i < n; i++)
            {
                holder[i, idx] = tmp[i];
            }
        }

        public static void tmpInsertRow(double[,] holder, double[] tmp, int idx)
        {
            int n = tmp.Length;

            for (int i = 0; i < n; i++)
            {
                holder[idx, i] = tmp[i];
            }
        }

        public static double[] xToArray(List<Point> zeroRates)
        {
            double[] tmp = new double[zeroRates.Count];
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = zeroRates[i].x;
            }
            return tmp;
        }

        public static double[] yToArray(List<Point> zeroRates)
        {
            double[] tmp = new double[zeroRates.Count];
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = zeroRates[i].y;
            }
            return tmp;
        }

        public static object[,] Transpose<T>(IEnumerable<T> a)
        {
            object[,] res = new object[a.Count(), 1];
            int i = 0;

            foreach (var item in a)
            {
                res[i,0] = item;
                i++;
            }

            return res;
        }
        public static System.Collections.Generic.Dictionary<string, object> ParseInstrument(string instrument, DateTime Anchor)
        {
            // Parse input instrument such as SWAP 10Y EUR6M into type:SWAP, tenor: 10y, index: EUR6M fx

            var parsedInstrument = new Dictionary<string, object>();
            parsedInstrument["anchor"] = Anchor;
            string[] inputs = instrument.Split(' ');

            int inputLenght = inputs.Length;

            // Check type first
            string type = inputs[0].ToLower();

            switch (type)
            {
                case "swap":

                    parsedInstrument.Add("type", "swap");
                    // check if at market or off market swap
                    // if off market, the last element of the input should be the rate
                    // that is, if the last element can be converted to double, it is a rate and therefore an off market swap

                    if (isNumeric(inputs.Last()))
                    {
                        // Last input is numeric, so we assume it is an off market instrument
                        // Next step is to check whether is is a spot starting or forward starting instrument
                        // we do this by counting the number of input arguments

                        if (inputLenght == 4)
                        {
                            // spot starting
                            parsedInstrument.Add("start", "2b");
                            parsedInstrument.Add("tenor", inputs[1]);
                            parsedInstrument.Add("index", inputs[2].ToLower());
                        }
                        else if (inputLenght == 5)
                        {
                            // forward starting
                            parsedInstrument.Add("start", inputs[1]);
                            parsedInstrument.Add("tenor", inputs[2]);
                            parsedInstrument.Add("index", inputs[3].ToLower());

                        }
                        else
                        {
                            // Throw exception here
                        }

                        // Lastly add quote to parsed instrument
                        parsedInstrument.Add("quote", inputs.Last());


                    }
                    else
                    {
                        // Last input is not numeric, so we assume it is an at market instrument
                        // Next step is to checke whether it is a spot starting or forward starting insturment
                        // we do this by counting the number of input arguments

                        if (inputLenght == 3)
                        {
                            // spot starting 
                            parsedInstrument.Add("start", "2b");
                            parsedInstrument.Add("tenor", inputs[1]);
                            parsedInstrument.Add("index", inputs[2].ToLower());
                        }
                        else if (inputLenght == 4)
                        {
                            // Forward starting
                            parsedInstrument.Add("start", inputs[1]);
                            parsedInstrument.Add("tenor", inputs[2]);
                            parsedInstrument.Add("index", inputs[3].ToLower());
                        }
                        else
                        {
                            // Throw Exception here
                        }

                        // Lastly add quote to parsed instrument - Since we are at market, we add empty string
                        parsedInstrument.Add("quote", "");
                    }

                    break;
                case "bond":
                    break;
                default:
                    break; // Throw exception here 
            }

            return parsedInstrument;
        }

        public static bool isNumeric(string str)
        {
            // Auxillary function to help determine whether a string can be parsed as a double.
            // This is used in determining whether some input is a rate/quote
            double result = 0;
            return (double.TryParse(str, System.Globalization.NumberStyles.Float,
                    System.Globalization.NumberFormatInfo.CurrentInfo, out result));
        }


        public static int LeftSegmentIndex<T>(T[] array, T t)
        {
            int index = Array.BinarySearch<T>(array, t);
            if (index < 0)
            {
                index = ~index - 1;
            }
            return System.Math.Min(System.Math.Max(index, 0), array.Length - 2);
        }

        public static object[,] Risk(Portfolio port, CurveModel model)
        {

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







    }


    public static class CurveModelHelper
    {
        public static double Annuity(CurveModel curve, DateSchedule d, string DayCount)
        {
            double tmp_fixed = 0;

            foreach (var row in d.dates)
            {
                tmp_fixed += row.Cvg(DayCount) * curve.DiscFactor(row.adjEnd, DayCount);
            }

            return tmp_fixed;
        }

        public static double Annuity(CurveModel curve, string start, string end, string freq, string dayrule,string daycount, BusinessCalendar calendar)
        {
            DateTime s = DateTimeUtils.AddTenor(curve.Anchor, start, calendar, dayrule);
            DateTime e = DateTimeUtils.AddTenor(s, end, calendar, dayrule);
            DateSchedule d = new DateSchedule(s, e, calendar, freq, dayrule);

            return Annuity(curve,d,daycount);
        }

        public static double Annuity(CurveModel curve, DateTime start, DateTime end, string freq, string dayrule, string daycount, BusinessCalendar calendar)
        {
            DateSchedule d = new DateSchedule(start, end, calendar, freq, dayrule);
            return Annuity(curve, d, daycount);
        }
    }




}
