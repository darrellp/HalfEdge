using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MeshNav
{
    public static class Utilities
    {
        internal static readonly VectorBuilder<double> DblBuilder = Vector<double>.Build;
        internal static readonly VectorBuilder<int> IntBuilder = Vector<int>.Build;
        internal static readonly VectorBuilder<float> FloatBuilder = Vector<float>.Build;

        public static Vector<double> ToDouble(this Vector<float> v)
        {
            var newVals = v.Select(val => (double) val).ToArray();
            return DblBuilder.Dense(newVals);
        }

        public static Vector<double> ToDouble(this Vector<int> v)
        {
            var newVals = v.Select(val => (double)val).ToArray();
            return DblBuilder.Dense(newVals);
        }

        public static Vector<double> ToDouble<T>(this Vector<T> v) where T : struct, IEquatable<T>, IFormattable
        {
            throw new MeshNavException("Invalid type for double conversion");
        }

        public static Vector<double> CrossProduct<T>(Vector<T> v1T, Vector<T> v2T)
            where T : struct, IEquatable<T>, IFormattable
        {
            if ((v1T.Count != 3 || v2T.Count != 3))
            {
                throw new MeshNavException("CrossProduct without length 3");
            }
            var v1 = ToDouble(v1T);
            var v2 = ToDouble(v2T);
            Vector<double> result = new DenseVector(3);
            result[0] = v1[1] * v2[2] - v1[2] * v2[1];
            result[1] = -v1[0] * v2[2] + v1[2] * v2[0];
            result[2] = v1[0] * v2[1] - v1[1] * v2[0];

            return result;
        }
    }
}
