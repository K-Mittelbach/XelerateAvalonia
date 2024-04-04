using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace XelerateAvalonia.Auxiliaries
{
    public class DataTransformations
    {
        public static double[][] MatStandardize(double[][] data,
     out double[] means, out double[] stds)
        {
            // scikit style z-score biased normalization
            int rows = data.Length;
            int cols = data[0].Length;
            double[][] result = MatCreate(rows, cols);

            // compute means
            double[] mns = new double[cols];
            for (int j = 0; j < cols; ++j)
            {
                double sum = 0.0;
                for (int i = 0; i < rows; ++i)
                    sum += data[i][j];
                mns[j] = sum / rows;
            } // j

            // compute std devs
            double[] sds = new double[cols];
            for (int j = 0; j < cols; ++j)
            {
                double sum = 0.0;
                for (int i = 0; i < rows; ++i)
                    sum += (data[i][j] - mns[j]) *
                      (data[i][j] - mns[j]);
                sds[j] = Math.Sqrt(sum / rows);  // biased
            } // j

            // normalize
            for (int j = 0; j < cols; ++j)
            {
                for (int i = 0; i < rows; ++i)
                    result[i][j] = (data[i][j] - mns[j]) / sds[j];
            } // j

            means = mns;
            stds = sds;

            return result;
        }
        public static double[][] MatCreate(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        // ------------------------------------------------------

        public static double[][] CovarMatrix(double[][] data,
          bool rowVar)
        {
            // rowVar == true means each row is a variable
            // if false, each column is a variable

            double[][] source;
            if (rowVar == true)
                source = data;  // by ref
            else
                source = MatTranspose(data);

            int srcRows = source.Length;  // num features
            int srcCols = source[0].Length;  // not used

            double[][] result = MatCreate(srcRows, srcRows);

            for (int i = 0; i < result.Length; ++i)
            {
                for (int j = 0; j <= i; ++j)
                {
                    result[i][j] = Covariance(source[i], source[j]);
                    result[j][i] = result[i][j];
                }
            }

            return result;
        }

        // ------------------------------------------------------

        public static double Covariance(double[] v1, double[] v2)
        {
            // compute means of v1 and v2
            int n = v1.Length;

            double sum1 = 0.0;
            for (int i = 0; i < n; ++i)
                sum1 += v1[i];
            double mean1 = sum1 / n;

            double sum2 = 0.0;
            for (int i = 0; i < n; ++i)
                sum2 += v2[i];
            double mean2 = sum2 / n;

            // compute covariance
            double sum = 0.0;
            for (int i = 0; i < n; ++i)
                sum += (v1[i] - mean1) * (v2[i] - mean2);
            double result = sum / (n - 1);

            return result;
        }

        // ------------------------------------------------------

        public static double[][] MatTranspose(double[][] M)
        {
            int nr = M.Length;
            int nc = M[0].Length;
            double[][] result = MatCreate(nc, nr);  // note
            for (int i = 0; i < nr; ++i)
                for (int j = 0; j < nc; ++j)
                    result[j][i] = M[i][j];
            return result;
        }

        // ------------------------------------------------------

        public static void Eigen(double[][] A,
          out double[] eval, out double[][] evec)
        {
            // Jacobi algorithm based on GSL implementation
            // assumes A is square symmetric
            // OK because being applied to a covariance matrix
            int m = A.Length; int n = A[0].Length;
            // if m != n throw an exception
            int maxRot = 100 * m * m;  // heuristic
            double redSum = 0.0;

            eval = new double[m];
            evec = MatIdentity(m);

            for (int i = 0; i < maxRot; ++i)
            {
                double nrm = Normalize(A);

                if (nrm == 0.0)  // mildly risky
                    break;

                for (int p = 0; p < n; ++p)
                {
                    for (int q = p + 1; q < n; ++q)
                    {
                        double c; double s;
                        redSum += Symschur2(A, p, q, out c, out s);
                        // Compute A := J^T A J 
                        Apply_Jacobi_L(A, p, q, c, s);
                        Apply_Jacobi_R(A, p, q, c, s);
                        // Compute V := V J 
                        Apply_Jacobi_R(evec, p, q, c, s);
                    }
                }
            }

            // nrot = i;
            for (int p = 0; p < n; ++p)
            {
                double ep = A[p][p];
                eval[p] = ep;
            }
        }

        // ------------------------------------------------------

        public static double[][] MatIdentity(int n)
        {
            double[][] result = MatCreate(n, n);
            for (int i = 0; i < n; ++i)
                result[i][i] = 1.0;
            return result;
        }

        // ------------------------------------------------------

        public static double Symschur2(double[][] A, int p, int q,
          out double c, out double s)
        {
            // Symmetric Schur decomposition 2x2 matrix
            double Apq = A[p][q];
            if (Apq != 0.0)
            {
                double App = A[p][p];
                double Aqq = A[q][q];
                double tau = (Aqq - App) / (2.0 * Apq);
                double t, c1;

                if (tau >= 0.0)
                    t = 1.0 / (tau + Hypot(1.0, tau));
                else
                    t = -1.0 / (-tau + Hypot(1.0, tau));

                c1 = 1.0 / Hypot(1.0, t);
                c = c1; s = t * c1;
            }
            else  // Apq == 0.0
            {
                c = 1.0; s = 0.0;
            }

            return Math.Abs(Apq);
        }

        // ------------------------------------------------------

        public static double Hypot(double x, double y)
        {
            // wacky sqrt(x^2 + y^2)
            double xabs = Math.Abs(x);
            double yabs = Math.Abs(y);
            double min, max;

            if (xabs < yabs)
            {
                min = xabs; max = yabs;
            }
            else
            {
                min = yabs; max = xabs;
            }

            if (min == 0)
                return max;

            double u = min / max;
            return max * Math.Sqrt(1 + u * u);
        }

        // ------------------------------------------------------

        public static void Apply_Jacobi_L(double[][] A, int p,
          int q, double c, double s)
        {
            int n = A[0].Length;

            // Apply rotation to matrix A,  A' = J^T A 
            for (int j = 0; j < n; ++j)
            {
                double Apj = A[p][j];
                double Aqj = A[q][j];
                A[p][j] = Apj * c - Aqj * s;
                A[q][j] = Apj * s + Aqj * c;
            }
        }

        // ------------------------------------------------------

        public static void Apply_Jacobi_R(double[][] A, int p,
          int q, double c, double s)
        {
            int m = A.Length;

            // Apply rotation to matrix A,  A' = A J 
            for (int i = 0; i < m; ++i)
            {
                double Aip = A[i][p];
                double Aiq = A[i][q];
                A[i][p] = Aip * c - Aiq * s;
                A[i][q] = Aip * s + Aiq * c;
            }
        }

        // ------------------------------------------------------

        public static double Normalize(double[][] A)
        {
            int m = A.Length; int n = A[0].Length;
            double scale = 0.0;
            double ssq = 1.0;

            for (int i = 0; i < m; ++i)
            {
                for (int j = 0; j < n; j++)
                {
                    double Aij = A[i][j];

                    // compute norm of off-diagonal elements
                    if (i == j) continue;

                    if (Aij != 0.0)
                    {
                        double ax = Math.Abs(Aij);

                        if (scale < ax)
                        {
                            ssq =
                              1.0 + ssq * (scale / ax) * (scale / ax);
                            scale = ax;
                        }
                        else
                        {
                            ssq += (ax / scale) * (ax / scale);
                        }
                    }

                } // j
            } // i

            double sum = scale * Math.Sqrt(ssq);
            return sum;
        }

        // ------------------------------------------------------

        public static int[] ArgSort(double[] vec)
        {
            int n = vec.Length;
            int[] idxs = new int[n];
            for (int i = 0; i < n; ++i)
                idxs[i] = i;
            Array.Sort(vec, idxs);  // sort idxs based on vec vals
            return idxs;
        }

        // ------------------------------------------------------

        public static double[][] MatExtractCols(double[][] mat,
          int[] cols)
        {
            int srcRows = mat.Length;
            int srcCols = mat[0].Length;
            int tgtCols = cols.Length;

            double[][] result = MatCreate(srcRows, tgtCols);
            for (int i = 0; i < srcRows; ++i)
            {
                for (int j = 0; j < tgtCols; ++j)
                {
                    int c = cols[j];
                    result[i][j] = mat[i][c];
                }
            }
            return result;
        }

        // ------------------------------------------------------

        public static double[][] MatProduct(double[][] matA,
          double[][] matB)
        {
            int aRows = matA.Length;
            int aCols = matA[0].Length;
            int bRows = matB.Length;
            int bCols = matB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices");

            double[][] result = MatCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k)
                        result[i][j] += matA[i][k] * matB[k][j];

            return result;
        }

        // ------------------------------------------------------

        public static double[] VarExplained(double[] eigenVals)
        {
            // assumes eigenVals are sorted large to small
            int n = eigenVals.Length;
            double[] result = new double[n];
            double sum = 0.0;
            for (int i = 0; i < n; ++i)
                sum += eigenVals[i];
            for (int i = 0; i < n; ++i)
            {
                double pctExplained = eigenVals[i] / sum;
                result[i] = pctExplained;
            }
            return result;
        }

    }
}
