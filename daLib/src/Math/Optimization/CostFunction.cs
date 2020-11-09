

using System;
using System.Linq;

namespace daLib.Math.Optimization
{
    public abstract class CostFunction
    {
        //! method to overload to compute the cost function value in x
        public virtual double value(Vector x)
        {
            Vector v = Vector.Sqrt(x);
            return System.Math.Sqrt(v.Sum(a => a) / Convert.ToDouble(v.size()));
        }
        //! method to overload to compute the cost function values in x
        public abstract Vector values(Vector x);

        //! method to overload to compute grad_f, the first derivative of
        //  the cost function with respect to x
        public virtual void gradient(ref Vector grad, Vector x)
        {
            double eps = finiteDifferenceEpsilon(), fp, fm;
            Vector xx = new Vector(x);
            for (int i = 0; i < x.Count; i++)
            {
                xx[i] += eps;
                fp = value(xx);
                xx[i] -= 2.0 * eps;
                fm = value(xx);
                grad[i] = 0.5 * (fp - fm) / eps;
                xx[i] = x[i];
            }
        }

        //! method to overload to compute grad_f, the first derivative of
        //  the cost function with respect to x and also the cost function
        public virtual double valueAndGradient(ref Vector grad, Vector x)
        {
            gradient(ref grad, x);
            return value(x);
        }

        //! method to overload to compute J_f, the jacobian of
        // the cost function with respect to x
        public virtual void jacobian(Matrix jac, Vector x)
        {
            double eps = finiteDifferenceEpsilon();
            Vector xx = new Vector(x);
            Vector fp = new Vector();
            Vector fm = new Vector();
            for (int i = 0; i < x.size(); ++i)
            {
                xx[i] += eps;
                fp = values(xx);
                xx[i] -= 2.0 * eps;
                fm = values(xx);
                for (int j = 0; j < fp.size(); ++j)
                {
                    jac[j, i] = 0.5 * (fp[j] - fm[j]) / eps;
                }
                xx[i] = x[i];
            }
        }

        //! method to overload to compute J_f, the jacobian of
        // the cost function with respect to x and also the cost function
        public virtual Vector valuesAndJacobian(Matrix jac, Vector x)
        {
            jacobian(jac, x);
            return values(x);
        }

        //! Default epsilon for finite difference method :
        public virtual double finiteDifferenceEpsilon() { return 1e-8; }
    }

    public interface IParametersTransformation
    {
        Vector direct(Vector x);
        Vector inverse(Vector x);
    }
}
