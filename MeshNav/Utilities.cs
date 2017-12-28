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
	}
}
