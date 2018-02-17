using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using MeshNav.Placement;
using Templates;

namespace MeshNavTests
{
	[TestClass]
	public class TestPlacement
	{
		private Face BuildParallelogram(Mesh mesh)
		{
			// ReSharper disable InconsistentNaming
			var ptLL = mesh.AddVertex(0, 0);
			var ptLR = mesh.AddVertex(2, 0);
			var ptUR = mesh.AddVertex(3, 1);
			var ptUL = mesh.AddVertex(1, 1);
			// ReSharper restore InconsistentNaming
			return mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
		}

		[TestMethod]
		public void TestPlacementGeneralCase()
		{
			// No two points share the same X position
			var mesh = new BndFactory(2).CreateMesh() as BndMesh;

			var face = BuildParallelogram(mesh);
			mesh.FinalizeMesh();

			var tree = Placement.GetPlacementTree(mesh);
			Assert.IsNotNull(tree.Locate(0.5, 0.1));
			Assert.IsNotNull(tree.Locate(1.5, 0.1));

			Assert.IsNotNull(tree.Locate(2.5, 0.9));

			Assert.IsNull(tree.Locate(-1, 0));

			Assert.IsNull(tree.Locate(0.5, -1));

			Assert.IsNull(tree.Locate(0.5, 3));
			Assert.IsNull(tree.Locate(1.5, -1));
			Assert.IsNull(tree.Locate(1.5, 3));
			Assert.IsNull(tree.Locate(2.5, 0.1));
			Assert.IsNull(tree.Locate(2.5, 3));
		}
	}
}
