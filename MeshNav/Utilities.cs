using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
	public static class Utilities
	{
        // Cross Product
		public static Vector<T> CrossProduct(Vector<T> v1, Vector<T> v2)
		{
			if ((v1.Count != 3 || v2.Count != 3))
			{
				throw new MeshNavException("CrossProduct without length 3");
			}
			var result = Factory.Builder.Dense(3);
			result[0] = v1[1] * v2[2] - v1[2] * v2[1];
			result[1] = -v1[0] * v2[2] + v1[2] * v2[0];
			result[2] = v1[0] * v2[1] - v1[1] * v2[0];
			return result;
		}

        // Linq
        public static T FirstOr(this IEnumerable<T> source, Func<T, bool> selector, T alternate)
        {
            foreach (var t in source.Where(selector))
                return t;
            return alternate;
        }

	    public static Vector<T> Make(T x = 0, T y = 0)
	    {
	        return Factory.Builder.DenseOfArray(new[] {x, y});
	    }


		public static T Slope(Vector<T> left, Vector<T> right)
		{
			T slope;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (left.X() == right.X())
			{
				slope = left.Y() > right.Y() ? T.MinValue : T.MaxValue;
			}
			else
			{
				slope = (right.Y() - left.Y()) / (right.X() - left.X());
			}

			return slope;
		}

        #region Extensions
		#region X
		public static T X(this Vector<T> vec)
	    {
	        return vec[0];
	    }
        #endregion

	    #region Y
	    public static T Y(this Vector<T> vec)
	    {
	        return vec[1];
	    }
        #endregion

        #region Vector Extensions
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns a vector 90 degrees in a CCW direction from the original. </summary>
        ///
        /// <remarks>	Darrellp, 2/27/2011. </remarks>
        ///
        /// <returns>	Vector 90 degrees from original. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Vector<T> Flip90Ccw(this Vector<T> vec)
        {
	        return Make(-vec.Y(), vec.X());
	    }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	returns the distance from the origin to the point. </summary>
        ///
        /// <remarks>	Darrellp, 2/27/2011. </remarks>
        ///
        /// <returns>	Length of the point considered as a vector. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static double Length(this Vector<T> vec)
	    {
            return Math.Sqrt(vec.X() * vec.X() + vec.Y() * vec.Y());
	    }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns a normalized version of the point. </summary>
        ///
        /// <remarks>	Darrellp, 2/27/2011. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Vector<T> Normalize(this Vector<T> vec)
	    {
            var ln = vec.Length();
	        // ReSharper disable RedundantCast
	        return Make(vec.X() / (T)ln, vec.Y() / (T)ln);
	        // ReSharper restore RedundantCast
	    }
        #endregion
        #endregion
    }
}
