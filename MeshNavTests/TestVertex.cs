using MeshNav;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MeshNavTests
{
	[TestClass]
	public class TestVertex
	{
		[TestMethod]
		public void TestEquals()
		{
			Assert.AreEqual(new Vector(3,4,5,6), new Vector(3,4,5,6));
		}
	}
}
