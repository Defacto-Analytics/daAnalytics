﻿
using daLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace daLib.Math
{
    public class Matrix
    {
        #region properties

        private int rows_, columns_;

        public int rows()
        {
            return rows_;
        }

        public int columns()
        {
            return columns_;
        }

        public bool empty()
        {
            return rows_ == 0 || columns_ == 0;
        }

        private double[] data_;

        public double this[int i, int j]
        {
            get
            {
                return data_[i * columns_ + j];
            }
            set
            {
                data_[i * columns_ + j] = value;
            }
        }

        public double this[int i]
        {
            get
            {
                return data_[i];
            }
            set
            {
                data_[i] = value;
            }
        }

        public Vector row(int r)
        {
            Vector result = new Vector(columns_);
            for (int i = 0; i < columns_; i++)
                result[i] = this[r, i];
            return result;
        }

        public Vector column(int c)
        {
            Vector result = new Vector(rows_);
            for (int i = 0; i < rows_; i++)
                result[i] = this[i, c];
            return result;
        }

        public Vector diagonal()
        {
            int arraySize = System.Math.Min(rows(), columns());
            Vector tmp = new Vector(arraySize);
            for (int i = 0; i < arraySize; i++)
                tmp[i] = this[i, i];
            return tmp;
        }

        public Vector GetRange(int start, int length)
        {
            return new Vector(data_.Skip(start).Take(length).ToList());
        }

        #endregion

        #region Constructors

        //! creates an empty matrix
        public Matrix() { }

        //! creates a matrix with the given dimensions
        public Matrix(int rows, int columns)
        {
            data_ = new double[rows * columns];
            rows_ = rows;
            columns_ = columns;
        }

        //! creates the matrix and fills it with <tt>value</tt>
        public Matrix(int rows, int columns, double value)
        {
            data_ = new double[rows * columns];
            for (int i = 0; i < data_.Length; i++)
                data_[i] = value;
            rows_ = rows;
            columns_ = columns;
        }

        public Matrix(Matrix from)
        {
            data_ = !from.empty() ? (double[])from.data_.Clone() : null;
            rows_ = from.rows_;
            columns_ = from.columns_;
        }

        public Matrix(double[,] m)
        {
            rows_ = m.GetUpperBound(0) + 1;
            columns_ = m.GetUpperBound(1) + 1;
            data_ = new double[rows_ * columns_];

            for (int i = 0; i < rows_; i++)
            {
                for (int j = 0; j < columns_; j++)
                {
                    this[i, j] = m[i, j];
                }
            }
        }   
        #endregion

        #region Algebraic operators

        /*! \pre all matrices involved in an algebraic expression must have the same size. */
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return operMatrix(ref m1, ref m2, (x, y) => x + y);
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return operMatrix(ref m1, ref m2, (x, y) => x - y);
        }

        public static Matrix operator *(double value, Matrix m1)
        {
            return operValue(ref m1, value, (x, y) => x * y);
        }

        public static Matrix operator /(double value, Matrix m1)
        {
            return operValue(ref m1, value, (x, y) => x / y);
        }

        public static Matrix operator *(Matrix m1, double value)
        {
            return operValue(ref m1, value, (x, y) => x * y);
        }

        public static Matrix operator /(Matrix m1, double value)
        {
            return operValue(ref m1, value, (x, y) => x / y);
        }

        private static Matrix operMatrix(ref Matrix m1, ref Matrix m2, Func<double, double, double> func)
        {
            if (m1.rows_ != m2.rows_ && m1.columns_ != m2.columns_)
            {
                throw new ExcelException("operation on matrices with different sizes");
            }

            Matrix result = new Matrix(m1.rows_, m1.columns_);
            for (int i = 0; i < m1.rows_; i++)
                for (int j = 0; j < m1.columns_; j++)
                    result[i, j] = func(m1[i, j], m2[i, j]);
            return result;
        }

        private static Matrix operValue(ref Matrix m1, double value, Func<double, double, double> func)
        {
            Matrix result = new Matrix(m1.rows_, m1.columns_);
            for (int i = 0; i < m1.rows_; i++)
                for (int j = 0; j < m1.columns_; j++)
                    result[i, j] = func(m1[i, j], value);
            return result;
        }

        public static Vector operator *(Vector v, Matrix m)
        {
            if (v.Count != m.rows())
            {
                throw new ExcelException("vectors and matrices with different sizes ("
                             + v.Count + ", " + m.rows() + "x" + m.columns() +
                             ") cannot be multiplied");
            }

            Vector result = new Vector(m.columns());
            for (int i = 0; i < result.Count; i++)
                result[i] = v * m.column(i);
            return result;
        }

        /*! \relates Matrix */
        public static Vector operator *(Matrix m, Vector v)
        {
            if (v.Count != m.rows())
            {
                throw new ExcelException("vectors and matrices with different sizes ("
                             + v.Count + ", " + m.rows() + "x" + m.columns() +
                             ") cannot be multiplied");
            }


            Vector result = new Vector(m.rows());
            for (int i = 0; i < result.Count; i++)
                result[i] = m.row(i) * v;
            return result;
        }

        /*! \relates Matrix */
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.columns() != m2.rows())
            {
                throw new ExcelException("matrices with different sizes (" +
                             m1.rows() + "x" + m1.columns() + ", " +
                             m2.rows() + "x" + m2.columns() + ") cannot be multiplied");
            }

            Matrix result = new Matrix(m1.rows(), m2.columns());
            for (int i = 0; i < result.rows(); i++)
                for (int j = 0; j < result.columns(); j++)
                    result[i, j] = m1.row(i) * m2.column(j);
            return result;
        }

        #endregion

        public static Matrix transpose(Matrix m)
        {
            Matrix result = new Matrix(m.columns(), m.rows());
            for (int i = 0; i < m.rows(); i++)
                for (int j = 0; j < m.columns(); j++)
                    result[j, i] = m[i, j];
            return result;
        }

        public static Matrix inverse(Matrix m)
        {
            if (m.rows() != m.columns())
            {
                throw new ExcelException("matrix is not square");
            }


            int n = m.rows();
            Matrix result = new Matrix(n, n);
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    result[i, j] = m[i, j];

            Matrix lum;
            int[] perm;
            decompose(m, out lum, out perm);

            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;

                double[] x = Helper(lum, b); //
                for (int j = 0; j < n; ++j)
                    result[j, i] = x[j];
            }
            return result;
        }

        // Crout's LU decomposition for matrix determinant and inverse
        // stores combined lower & upper in lum[][]
        // stores row permuations into perm[]
        // returns +1 or -1 according to even or odd number of row permutations
        // lower gets dummy 1.0s on diagonal (0.0s above)
        // upper gets lum values on diagonal (0.0s below)
        public static int decompose(Matrix m, out Matrix lum, out int[] perm)
        {
            int toggle = +1; // even (+1) or odd (-1) row permutatuions
            int n = m.rows_;

            // Make a copy of Matrix m into result Matrix lu
            lum = new Matrix(n, n);
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    lum[i, j] = m[i, j];


            // make perm[]
            perm = new int[n];
            for (int i = 0; i < n; ++i)
                perm[i] = i;

            for (int j = 0; j < n - 1; ++j) // process by column. note n-1
            {
                double max = System.Math.Abs(lum[j, j]);
                int piv = j;

                for (int i = j + 1; i < n; ++i) // find pivot index
                {
                    double xij = System.Math.Abs(lum[i, j]);
                    if (xij > max)
                    {
                        max = xij;
                        piv = i;
                    }
                } // i

                if (piv != j)
                {
                    lum.swapRow(piv, j);

                    int t = perm[piv]; // swap perm elements
                    perm[piv] = perm[j];
                    perm[j] = t;

                    toggle = -toggle;
                }

                double xjj = lum[j, j];
                if (xjj == 0.0)
                {
                    for (int i = j + 1; i < n; ++i)
                    {
                        double xij = lum[i, j] / xjj;
                        lum[i, j] = xij;
                        for (int k = j + 1; k < n; ++k)
                            lum[i, k] -= xij * lum[j, k];
                    }
                }
            } // j

            return toggle;
        }

        public static double[] Helper(Matrix luMatrix, double[] b) // helper
        {
            int n = luMatrix.rows_;
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1, n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum / luMatrix[i, i];
            }

            return x;
        } // Helper

        public static Matrix outerProduct(List<double> v1begin, List<double> v2begin)
        {
            int size1 = v1begin.Count;
            if (size1 <= 0)
            {
                throw new ExcelException("null first vector");
            }

            int size2 = v2begin.Count;
            if (size1 <= 0)
            {
                throw new ExcelException("null second vector");
            }
            Matrix result = new Matrix(size1, size2);

            for (int i = 0; i < v1begin.Count; i++)
                for (int j = 0; j < v2begin.Count; j++)
                    result[i, j] = v1begin[i] * v2begin[j];
            return result;
        }

        public void fill(double value)
        {
            for (int i = 0; i < data_.Length; i++)
                data_[i] = value;
        }

        public void swap(int i1, int j1, int i2, int j2)
        {
            double t = this[i2, j2];
            this[i2, j2] = this[i1, j1];
            this[i1, j1] = t;
        }

        public void swapRow(int r1, int r2)
        {
            Vector t = row(r1);
            for (int i = 0; i < columns_; i++)
                this[r1, i] = this[r2, i];

            for (int i = 0; i < columns_; i++)
                this[r2, i] = t[i];
        }

        public double[,] ToArray()
        {
            double[,] tmp = new double[this.rows_, this.columns_];

            for (int i = 0; i < this.rows_; i++)
            {
                for (int j = 0; j < this.columns_; j++)
                {
                    tmp[i, j] = this[i, j];
                }
            }
            return tmp;
        }

        public override string ToString()
        {
            string to = string.Empty;
            for (int i = 0; i < rows(); i++)
            {
                to += "| ";
                for (int j = 0; j < columns(); j++)
                    to += this[i, j] + " ";
                to += "|" + Environment.NewLine;
            }
            return to;
        }
    }
}