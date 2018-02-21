using System;
using System.Collections.Generic;
using System.Linq;
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
		public static Vector CrossProduct(Vector v1, Vector v2)
		{
			if ((v1.Rank != 3 || v2.Rank != 3))
			{
				throw new MeshNavException("CrossProduct without length 3");
			}

			var result = new Vector(3)
			{
				X = v1[1] * v2[2] - v1[2] * v2[1],
				Y = -v1[0] * v2[2] + v1[2] * v2[0],
				Z = v1[0] * v2[1] - v1[1] * v2[0]
			};
			return result;
		}

        // Linq
        public static T FirstOr(this IEnumerable<T> source, Func<T, bool> selector, T alternate)
        {
            foreach (var t in source.Where(selector))
                return t;
            return alternate;
        }

		public static T Slope(T x1, T y1, T x2, T y2)
		{
			T slope;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (x1 == x2)
			{
				slope = y1 > y2 ? T.MinValue : T.MaxValue;
			}
			else
			{
				slope = (y2 - y1) / (x2 - x1);
			}

			return slope;
		}
		public static T Slope(Vector left, Vector right)
		{
			return Slope(left.X, left.Y, right.X, right.Y);
		}
    }
}
