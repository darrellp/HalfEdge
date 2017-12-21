using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace MeshNav
{
	public static class Utilities
	{
		internal static readonly VectorBuilder<double> DblBuilder = Vector<double>.Build;
		internal static readonly VectorBuilder<float> FloatBuilder = Vector<float>.Build;

        // ToDouble
		public static Vector<double> ToDouble(this Vector<float> v)
		{
			var newVals = v.Select(val => (double) val).ToArray();
			return DblBuilder.Dense(newVals);
		}

		public static Vector<double> ToDouble(this Vector<double> v)
		{
			return DblBuilder.DenseOfVector(v);
		}

		public static Vector<double> ToDouble<T>(this Vector<T> v) where T : struct, IEquatable<T>, IFormattable
		{
			throw new MeshNavException("Invalid type for double conversion");
		}

        // Cross Product
		public static Vector<double> CrossProduct(Vector<double> v1, Vector<double> v2)
		{
			if ((v1.Count != 3 || v2.Count != 3))
			{
				throw new MeshNavException("CrossProduct without length 3");
			}
			var result = DblBuilder.Dense(3);
			result[0] = v1[1] * v2[2] - v1[2] * v2[1];
			result[1] = -v1[0] * v2[2] + v1[2] * v2[0];
			result[2] = v1[0] * v2[1] - v1[1] * v2[0];
			return result;
		}

		public static Vector<float> CrossProduct(Vector<float> v1, Vector<float> v2)
		{
			if ((v1.Count != 3 || v2.Count != 3))
			{
				throw new MeshNavException("CrossProduct without length 3");
			}
			var result = FloatBuilder.Dense(3);
			result[0] = v1[1] * v2[2] - v1[2] * v2[1];
			result[1] = -v1[0] * v2[2] + v1[2] * v2[0];
			result[2] = v1[0] * v2[1] - v1[1] * v2[0];
			return result;
		}

		public static Vector<T> CrossProduct<T>(Vector<T> v1, Vector<T> v2) where T : struct, IEquatable<T>, IFormattable
		{
			throw new MeshNavException("Invalid type in CrossProduct");
		}

        // Scalar Divide
		public static Vector<double> ScalarDivide(this Vector<double> v, double d)
		{
			return DblBuilder.DenseOfEnumerable(v.Select(val => val / d));
		}

		public static Vector<float> ScalarDivide(this Vector<float> v, double d)
		{
			return FloatBuilder.DenseOfEnumerable(v.Select(val => (float)(val / d)));
		}

		public static Vector<T> ScalarDivide<T>(this Vector<T> v, double d) where T : struct, IEquatable<T>, IFormattable
		{
			throw new MeshNavException("Invalid type in CrossProduct");
		}

        // Scalar Multiply
		public static Vector<double> ScalarMultiply(this Vector<double> v, double d)
		{
			return DblBuilder.DenseOfEnumerable(v.Select(val => val * d));
		}

		public static Vector<float> ScalarMultiply(this Vector<float> v, double d)
		{
			return FloatBuilder.DenseOfEnumerable(v.Select(val => (float)(val * d)));
		}

		public static Vector<T> ScalarMultiply<T>(this Vector<T> v, double d) where T : struct, IEquatable<T>, IFormattable
		{
			throw new MeshNavException("Invalid type in CrossProduct");
		}

        // Linq
        public static T FirstOr<T>(this IEnumerable<T> source, Func<T, bool> selector, T alternate)
        {
            foreach (T t in source.Where(selector))
                return t;
            return alternate;
        }
	}
}
