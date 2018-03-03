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
		public void TestCrossProduct()
		{
			var vecX = new Vector( 1, 0, 0 );
			var vecY = new Vector(0, 1, 0 );
			var vecCp = Utilities.CrossProduct(vecX, vecY);
			Assert.AreEqual(0.0, vecCp[0]);
			Assert.AreEqual(0.0, vecCp[1]);
			Assert.AreEqual(1.0, vecCp[2]);
		}
	}
}