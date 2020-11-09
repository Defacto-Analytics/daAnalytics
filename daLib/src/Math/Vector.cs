﻿
using daLib.Exceptions;
using daLib.Patterns;
using System;
using System.Collections.Generic;

namespace daLib.Math
{
    /// <summary>
    /// 1-D vector used in linear algebra.
    /// </summary>
    /// <remarks>This class implements the concept of vector as used in linear algebra.
    /// As such, it is not meant to be used as a container -
    /// <c>List</c> should be used instead.</remarks>
    public class Vector : InitializedList<double>, IDeepClone<Vector>
    {
        /// <summary>
        /// Creates an empty Vector.
        /// </summary>
        public Vector() : this(0)
        { }

        /// <summary>
        /// Creates a Vector of the given size.
        /// </summary>
        public Vector(int size) : base(size)
        {
        }

        public Vector(int size, double value) : base(size)
        {
            for (int i = 0; i < size; i++)
                this.Add(value);
        }

        /// <summary>
        /// Creates the vector and fills it according to
        /// <para>Vector[0] = value</para>
        /// Vector[i]=Vector[i-1]+increment
        /// </summary>
        public Vector(int size, double value, double increment) : this(size)
        {
            for (int i = 0; i < Count; i++, value += increment)
                this[i] = value;
        }

        /// <summary>
        /// Creates a Vector cloning from
        /// </summary>
        public Vector(Vector from) : base(from.Count)
        {
            for (int i = 0; i < Count; i++)
                this[i] = from[i];
        }

        /// <summary>
        /// Creates a Vector as a copy of a given List
        /// </summary>
        public Vector(List<double> from) : base(from.Count)
        {
            for (int i = 0; i < Count; i++)
                this[i] = from[i];
        }

        public Vector(double[] from) : base(from.Length)
        {
            for (int i = 0; i < Count; i++)
                this[i] = from[i];
        }


        /// <summary>
        /// Returns a deep-copy clone of the Vector.
        /// </summary>
        /// <returns>A clone of the vector.</returns>
        public Vector Clone()
        {
            return new Vector(this);
        }

        /// <summary>
        /// Indicates whether the current Vector is equal to another Vector.
        /// </summary>
        /// <param name="other">A Vector to compare with this Vector.</param>
        /// <returns>
        ///    <c>true</c> if the current Vector is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// 
        public bool Equals(Vector other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (Count != other.Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (other[i] == this[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="o">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public sealed override bool Equals(object o)
        {
            var v = o as Vector;
            return v != null && this == v;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (var i = 0; i < Count; i++)
            {
                hash = hash * 31 + this[i].GetHashCode();
            }
            return hash;
        }

        public int size()
        {
            return Count;
        }

        public bool empty()
        {
            return Count == 0;
        }

        #region Vector algebra

        //    <tt>v += x</tt> and similar operation involving a scalar value
        //    are shortcuts for \f$ \forall i : v_i = v_i + x \f$

        //    <tt>v *= w</tt> and similar operation involving two vectors are
        //    shortcuts for \f$ \forall i : v_i = v_i \times w_i \f$

        //    \pre all arrays involved in an algebraic expression must have the same size.
        //
        public static Vector operator +(Vector v1, Vector v2)
        {
            return operVector(v1, v2, (x, y) => x + y);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return operVector(v1, v2, (x, y) => x - y);
        }

        public static Vector operator +(Vector v1, double value)
        {
            return operValue(v1, value, (x, y) => x + y);
        }

        public static Vector operator -(Vector v1, double value)
        {
            return operValue(v1, value, (x, y) => x - y);
        }

        public static Vector operator +(double value, Vector v1)
        {
            return operValue(v1, value, (x, y) => x + y);
        }

        public static Vector operator -(double value, Vector v1)
        {
            return operValue(v1, value, (x, y) => y - x);
        }

        public static Vector operator *(double value, Vector v1)
        {
            return operValue(v1, value, (x, y) => x * y);
        }

        public static Vector operator *(Vector v1, double value)
        {
            return operValue(v1, value, (x, y) => x * y);
        }

        public static Vector operator /(Vector v1, double value)
        {
            return operValue(v1, value, (x, y) => x / y);
        }

        internal static Vector operVector(Vector v1, Vector v2, Func<double, double, double> func)
        {
            if (v1.Count != v2.Count)
            {
                throw new ExcelException("operation on vectors with different sizes (" + v1.Count + ", " + v2.Count);
            }

            Vector temp = new Vector(v1.Count);
            for (int i = 0; i < v1.Count; i++)
                temp[i] = func(v1[i], v2[i]);
            return temp;
        }

        private static Vector operValue(Vector v1, double value, Func<double, double, double> func)
        {
            Vector temp = new Vector(v1.Count);
            for (int i = 0; i < v1.Count; i++)
                temp[i] = func(v1[i], value);
            return temp;
        }

        public static double operator *(Vector v1, Vector v2)
        {
            if (v1.Count != v2.Count)
            {
                throw new ExcelException("operation on vectors with different sizes (" + v1.Count + ", " + v2.Count);
            }

            double result = 0;
            for (int i = 0; i < v1.Count; i++)
                result += v1[i] * v2[i];
            return result;
        }

        #endregion

        #region Vector utils

        // dot product. It is already overloaded in the vector. Thus for compatibility only
        public static double DotProduct(Vector v1, Vector v2)
        {
            return v1 * v2;
        }

        public static double Norm2(Vector v)
        {
            return System.Math.Sqrt(v * v);
        }

        public static Vector DirectMultiply(Vector v1, Vector v2)
        {
            return operVector(v1, v2, (x, y) => x * y);
        }

        public static Vector Sqrt(Vector v)
        {
            Vector result = new Vector(v.size());
            for (int i = 0; i < v.size(); i++)
            {
                result[i] = System.Math.Sqrt(v[i]);
            }
            return result;
        }

        public static Vector Abs(Vector v)
        {
            Vector result = new Vector(v.size());
            for (int i = 0; i < v.size(); i++)
            {
                result[i] = System.Math.Abs(v[i]);
            }
            return result;
        }

        public static Vector Exp(Vector v)
        {
            Vector result = new Vector(v.size());
            for (int i = 0; i < v.size(); i++)
            {
                result[i] = System.Math.Exp(v[i]);
            }
            return result;
        }

        public void swap(int i1, int i2)
        {
            double t = this[i2];
            this[i2] = this[i1];
            this[i1] = t;
        }

        public Vector DeepClone()
        {
            return new Vector(this);
        }   

        object IDeepClone.DeepClone()
        {
            return this.DeepClone();
        }

        #endregion
    }
}
