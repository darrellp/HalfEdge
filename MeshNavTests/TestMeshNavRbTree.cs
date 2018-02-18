using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
// ReSharper disable AccessToModifiedClosure
#endif

namespace MeshNavTests
{
	[TestClass]
	public class TestMeshNavRbTree
	{
		[TestMethod]
		public void TestConstructor()
		{
			var mnrb = new MeshNavRbTree();
			Assert.IsNotNull(mnrb);
		}

		[TestMethod]
		public void TestInsertOneItem()
		{
			var mnrb = new MeshNavRbTree();
			var sweep = 0;
			// From origin up 45 degrees to the right
			// ReSharper disable RedundantArgumentDefaultValue
			var rbt1 = new RbLineSegment(Make(0, 0), Make(100, 100), () => sweep);
			(var higher, var lower) = mnrb.InsertBracketed(rbt1);
			Assert.IsNull(higher);
			Assert.IsNull(lower);

			// 45 deg up from (0, 10)
			var rbt2 = new RbLineSegment(Make(0, 10), Make(100, 110), () => sweep);
			(higher, lower) = mnrb.InsertBracketed(rbt2);
			Assert.AreEqual(lower, rbt1);
			Assert.AreEqual(higher, null);

			// 45 deg up from (0, 5)
			var rbt3 = new RbLineSegment(Make(0, 5), Make(100, 105), () => sweep);
			(higher, lower) = mnrb.InsertBracketed(rbt3);
			Assert.AreEqual(lower, rbt1);
			Assert.AreEqual(higher, rbt2);

			// With sweepline at 0, a horizontal line at y = 1 should have rbt1 lower and rbt3 higher
			var rbthorz = new RbLineSegment(Make(0, 1), Make(100, 1), () => sweep);
			(higher, lower) = mnrb.InsertBracketed(rbthorz);
			Assert.AreEqual(lower, rbt1);
			Assert.AreEqual(higher, rbt3);

			// Start over and repeat
			mnrb = new MeshNavRbTree();

			rbt1 = new RbLineSegment(Make(0, 0), Make(100, 100), () => sweep);
			mnrb.InsertBracketed(rbt1);
			rbt2 = new RbLineSegment(Make(0, 10), Make(100, 110), () => sweep);
			mnrb.InsertBracketed(rbt2);
			rbt3 = new RbLineSegment(Make(0, 5), Make(100, 105), () => sweep);
			mnrb.InsertBracketed(rbt3);
			// ReSharper enable RedundantArgumentDefaultValue

			// Now move the sweepline to 5 and the horizontal line at y=1 should have rbt1 above it and
			// nothing below it
			sweep = 5;
			rbthorz = new RbLineSegment(Make(0, 1), Make(100, 1), () => sweep);
			(higher, lower) = mnrb.InsertBracketed(rbthorz);
			Assert.AreEqual(lower, null);
			Assert.AreEqual(higher, rbt1);
		}
	}
}
