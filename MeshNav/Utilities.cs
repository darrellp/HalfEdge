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
			throw new MeshNavException("Invalid type in ScalarMultiply");
		}

        // Linq
        public static T FirstOr<T>(this IEnumerable<T> source, Func<T, bool> selector, T alternate)
        {
            foreach (T t in source.Where(selector))
                return t;
            return alternate;
        }

	    public static Vector<T> Make<T>(double x = 0, double y = 0) where T : struct, IEquatable<T>, IFormattable
	    {
	        if (typeof(T) == typeof(double))
	        {
	            return DblBuilder.DenseOfArray(new[] {x, y}) as Vector<T>;
	        }
	        if (typeof(T) == typeof(float))
	        {
	            return FloatBuilder.DenseOfArray(new[] { (float)x, (float)y }) as Vector<T>;
	        }
	        throw new MeshNavException("Invalid type in Make");
	    }

        #region Extensions
        #region X
        public static double X(this Vector<double> vec)
	    {
	        return vec[0];
	    }

	    public static float X(this Vector<float> vec)
	    {
	        return vec[0];
	    }

	    public static T X<T>(this Vector<T> vec) where T : struct, IEquatable<T>, IFormattable
        {
	        throw new MeshNavException("Invalid type in X");
	    }
        #endregion

	    #region Y
	    public static double Y(this Vector<double> vec)
	    {
	        return vec[0];
	    }

	    public static float Y(this Vector<float> vec)
	    {
	        return vec[0];
	    }

	    public static T Y<T>(this Vector<T> vec) where T : struct, IEquatable<T>, IFormattable
	    {
	        throw new MeshNavException("Invalid type in Y");
	    }
        #endregion

        // ReSharper disable InconsistentNaming
        #region XD
        public static double XD(this Vector<double> vec) => vec[0];
        public static double XD(this Vector<float> vec) => vec[0];
        public static double XD<T>(this Vector<T> vec) where T : struct, IEquatable<T>, IFormattable => throw new MeshNavException("Invalid type in YD");
        #endregion

        #region YD
        public static double YD(this Vector<double> vec) => vec[1];
        public static double YD(this Vector<float> vec) => vec[1];
        public static double YD<T>(this Vector<T> vec) where T : struct, IEquatable<T>, IFormattable => throw new MeshNavException("Invalid type in YD");
        #endregion
        // ReSharper restore InconsistentNaming

        ////////////////////////////////////////////////////////////////////////////////////////////////////
	    /// <summary>	Returns a vector 90 degrees in a CCW direction from the original. </summary>
	    ///
	    /// <remarks>	Darrellp, 2/27/2011. </remarks>
	    ///
	    /// <returns>	Vector 90 degrees from original. </returns>
	    ////////////////////////////////////////////////////////////////////////////////////////////////////

	    public static Vector<T> Flip90Ccw<T>(this Vector<T> vec) where T : struct, IEquatable<T>, IFormattable
        {
	        return Make<T>(-vec.YD(), vec.XD());
	    }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	returns the distance from the origin to the point. </summary>
        ///
        /// <remarks>	Darrellp, 2/27/2011. </remarks>
        ///
        /// <returns>	Length of the point considered as a vector. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static double Length<T>(this Vector<T> vec) where T : struct, IEquatable<T>, IFormattable
	    {
            return Math.Sqrt(vec.XD() * vec.XD() + vec.YD() * vec.YD());
	    }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns a normalized version of the point. </summary>
        ///
        /// <remarks>	Darrellp, 2/27/2011. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Vector<T> Normalize<T>(this Vector<T> vec) where T : struct, IEquatable<T>, IFormattable
	    {
            var ln = vec.Length<T>();
	        return Make<T>(vec.XD() / ln, vec.YD() / ln);
	    }
        #endregion
    }
}
