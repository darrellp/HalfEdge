using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNavTests
{
	[TestClass]
	public class TestSimplePolygon
	{
		[TestMethod]
		public void TestSimple()
		{
			var polylist = new List<Vector>
			{
				new Vector(0, 0),
				new Vector(4, 0),
				new Vector(3, 1),
				new Vector(5, 2),
				new Vector(1, 2),
				new Vector(2, 1)
			};
			Assert.IsTrue(SimplePolygon.FTestSimplePolygon(polylist));

			// Square
			polylist = new List<Vector>
			{
				new Vector(0, 0),
				new Vector(0, 1),
				new Vector(1, 1),
				new Vector(1, 0)
			};
			Assert.IsTrue(SimplePolygon.FTestSimplePolygon(polylist));

			// Almost the same as second case in TestNonSimple except we don't quite touch
			// the line
			polylist = new List<Vector>
			{
				new Vector(0, 0),
				new Vector(1, 1),
				// ReSharper disable once RedundantCast
				new Vector((T)0.01, 1),
				new Vector(1, 2),
				new Vector(0, 2)
			};
			Assert.IsTrue(SimplePolygon.FTestSimplePolygon(polylist));
		}

		[TestMethod]
		public void TestNonSimple()
		{
			var polylist = new List<Vector>
			{
				new Vector(0, 0),
				new Vector(4, 0),
				new Vector(2, 1),
				new Vector(5, 2),
				new Vector(1, 2),
				new Vector(3, 1),
			};
			Assert.IsFalse(SimplePolygon.FTestSimplePolygon(polylist));

			// Just barely touching the line
			polylist = new List<Vector>
			{
				new Vector(0, 0),
				new Vector(1, 1),
				new Vector(0, 1),
				new Vector(1, 2),
				new Vector(0, 2)
			};
			Assert.IsFalse(SimplePolygon.FTestSimplePolygon(polylist));
		}
	}
}
