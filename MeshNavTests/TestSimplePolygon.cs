using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
// ReSharper disable RedundantArgumentDefaultValue
#endif

namespace MeshNavTests
{
	[TestClass]
	public class TestSimplePolygon
	{
		[TestMethod]
		public void TestSimple()
		{
			var polylist = new List<Vector<T>>
			{
				Make(0, 0),
				Make(4, 0),
				Make(3, 1),
				Make(5, 2),
				Make(1, 2),
				Make(2, 1)
			};
			Assert.IsTrue(SimplePolygon.FTestSimplePolygon(polylist));

			// Square
			polylist = new List<Vector<T>>
			{
				Make(0, 0),
				Make(0, 1),
				Make(1, 1),
				Make(1, 0)
			};
			Assert.IsTrue(SimplePolygon.FTestSimplePolygon(polylist));

			// Almost the same as second case in TestNonSimple except we don't quite touch
			// the line
			polylist = new List<Vector<T>>
			{
				Make(0, 0),
				Make(1, 1),
				Make(0.01, 1),
				Make(1, 2),
				Make(0, 2)
			};
			Assert.IsTrue(SimplePolygon.FTestSimplePolygon(polylist));
		}

		[TestMethod]
		public void TestNonSimple()
		{
			var polylist = new List<Vector<T>>
			{
				Make(0, 0),
				Make(4, 0),
				Make(2, 1),
				Make(5, 2),
				Make(1, 2),
				Make(3, 1),
			};
			Assert.IsFalse(SimplePolygon.FTestSimplePolygon(polylist));

			// Just barely touching the line
			polylist = new List<Vector<T>>
			{
				Make(0, 0),
				Make(1, 1),
				Make(0, 1),
				Make(1, 2),
				Make(0, 2)
			};
			Assert.IsFalse(SimplePolygon.FTestSimplePolygon(polylist));
		}
	}
}
