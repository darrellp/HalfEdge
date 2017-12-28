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
	public class TestUtilities
	{
		[TestMethod]
		public void TestBuilders()
		{
			var vec = Factory.Builder.DenseOfArray(new T[] {3, 4, 5});
			Assert.AreEqual(3.0, vec[0]);
			Assert.AreEqual(4.0, vec[1]);
			Assert.AreEqual(5.0, vec[2]);
		}

		[TestMethod]
		public void TestCrossProduct()
		{
			var vecX = Factory.Builder.DenseOfArray(new T[] { 1, 0, 0 });
			var vecY = Factory.Builder.DenseOfArray(new T[] { 0, 1, 0 });
			var vecCp = Utilities.CrossProduct(vecX, vecY);
			Assert.AreEqual(0.0, vecCp[0]);
			Assert.AreEqual(0.0, vecCp[1]);
			Assert.AreEqual(1.0, vecCp[2]);
		}
	}
}