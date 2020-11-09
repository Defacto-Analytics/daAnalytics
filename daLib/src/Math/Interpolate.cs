using System;

namespace daLib.Math
{
    public interface IInterpolate
    {
        double interpolate(double x);
        void updateInterpolater(double[] x, double[] y);
    }

    public class CubicSpline : IDeepClone<CubicSpline>
    {
        // Interpolation Methods for Curve Construction - Hagan & West (2006)
        // Bessel (Hermit) Cubic Spline, p99.
        readonly double[] x;

        // Cubic spline constants
        readonly double[] a;
        readonly double[] b;
        readonly double[] c;
        readonly double[] d;

        public CubicSpline(double[] x, double[] a, double[] b, double[] c, double[] d)
        {
            // check sizes
            if (x.Length != a.Length + 1 || x.Length != b.Length + 1 || x.Length != c.Length + 1 || x.Length != d.Length + 1)
            {
                throw new ArgumentException("Not appropriate length input arrays");
            }

            // Check size of x
            if (x.Length < 2)
            {
                throw new ArgumentException("Array to small", nameof(x));
            }

            this.x = x;
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }



        public static CubicSpline BuildHermiteInterpolaterSorted(double[] x, double[] y, double[] firstDerivatives)
        {
            if (x.Length != y.Length || x.Length != firstDerivatives.Length)
            {
                throw new ArgumentException("Not appropriate length input arrays");
            }

            var c0 = new double[x.Length - 1];
            var c1 = new double[x.Length - 1];
            var c2 = new double[x.Length - 1];
            var c3 = new double[x.Length - 1];
            for (int i = 0; i < c1.Length; i++)
            {
                double w = x[i + 1] - x[i];
                double w2 = w * w;
                c0[i] = y[i];
                c1[i] = firstDerivatives[i];
                c2[i] = (3 * (y[i + 1] - y[i]) / w - 2 * firstDerivatives[i] - firstDerivatives[i + 1]) / w;
                c3[i] = (2 * (y[i] - y[i + 1]) / w + firstDerivatives[i] + firstDerivatives[i + 1]) / w2;
            }

            return new CubicSpline(x, c0, c1, c2, c3);
        }

        public CubicSpline DeepClone()
        {
            double[] a = Helper.DeepCopyArrayOfValue<double>(this.a);
            double[] b = Helper.DeepCopyArrayOfValue<double>(this.b);
            double[] c = Helper.DeepCopyArrayOfValue<double>(this.c);
            double[] d = Helper.DeepCopyArrayOfValue<double>(this.d);
            double[] x = Helper.DeepCopyArrayOfValue<double>(this.x);

            return new CubicSpline(x, a, b, c, d);
        }

        public double Interpolate(double tau)
        {
            int i = LeftSegmentIndex(tau);
            var _x_ = tau - this.x[i];
            return a[i] + _x_ * (b[i] + _x_ * (c[i] + _x_ * d[i]));
        }

        object IDeepClone.DeepClone()
        {
            return this.DeepClone();
        }

        private int LeftSegmentIndex(double tau)
        {
            int index = Array.BinarySearch(this.x, tau);
            if (index < 0)
            {
                index = ~index - 1;
            }

            return System.Math.Min(System.Math.Max(index, 0), x.Length - 2);
        }
    }

    public static class InterpolationHelper
    {
        public static double[] BesselFirstDerivatives(double[] x, double[] y)
        {
            // Corresponds to p99 HagenWest

            int len = x.Length;
            double[] holder = new double[len];
            double temp;

            // Special cases for 0 and n-1
            temp = ((x[2] + x[1] - 2 * x[0]) * (y[1] - y[0]) / (x[1] - x[0]) - (x[1] - x[0]) * (y[2] - y[1]) / (x[2] - x[1])) * System.Math.Pow((x[2] - x[0]), -1);
            holder[0] = temp;

            temp = -((x[len - 1] - x[len - 2]) * (y[len - 2] - y[len - 3]) / (x[len - 2] - x[len - 3]) - (2 * x[len - 1] - x[len - 2] - x[len - 3]) * (y[len - 1] - y[len - 2]) / (x[len - 1] - x[len - 2])) * System.Math.Pow((x[len - 1] - x[len - 3]), -1);
            holder[len - 1] = temp;

            // Middle
            for (int i = 1; i < len - 1; i++)
            {
                temp = ((x[i + 1] - x[i]) * (y[i] - y[i - 1]) / (x[i] - x[i - 1]) + (x[i] - x[i - 1]) * (y[i + 1] - y[i]) / (x[i + 1] - x[i])) * System.Math.Pow((x[i + 1] - x[i - 1]), -1);
                holder[i] = temp;
            }

            return holder;
        }
    }
}