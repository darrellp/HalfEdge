using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using MeshNav.Placement;
using Templates;

namespace MeshNavTests
{
	[TestClass]
	public class TestPlacement
	{
		private Face BuildSquare(Mesh mesh)
		{
			// ReSharper disable InconsistentNaming
			var ptLL = mesh.AddVertex(0, 0);
			var ptLR = mesh.AddVertex(1, 0);
			var ptUL = mesh.AddVertex(0, 1);
			var ptUR = mesh.AddVertex(1, 1);
			// ReSharper restore InconsistentNaming
			return mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
		}

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

		private Face BuildTrapezoid(Mesh mesh)
		{
			// ReSharper disable InconsistentNaming
			var ptLL = mesh.AddVertex(0, 0);
			var ptLR = mesh.AddVertex(3, 0);
			var ptUR = mesh.AddVertex(2, 1);
			var ptUL = mesh.AddVertex(1, 1);
			// ReSharper restore InconsistentNaming
			return mesh.AddFace(ptLL, ptUL, ptUR, ptLR);
		}

		[TestMethod]
		public void TestPlacementGeneralEndpointCase()
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

		[TestMethod]
		public void TestPlacementGeneralMiddleCase()
		{
			// No two points share the same X position
			var mesh = new BndFactory(2).CreateMesh() as BndMesh;

			var face = BuildTrapezoid(mesh);
			mesh.FinalizeMesh();

			var tree = Placement.GetPlacementTree(mesh);
			Assert.IsNotNull(tree.Locate(0.5, 0.1));
			Assert.IsNotNull(tree.Locate(1.5, 0.1));
			Assert.IsNotNull(tree.Locate(2.5, 0.1));
			Assert.IsNull(tree.Locate(-1, 0));
			Assert.IsNull(tree.Locate(0.5, -1));
			Assert.IsNull(tree.Locate(0.5, 3));
			Assert.IsNull(tree.Locate(1.5, -1));
			Assert.IsNull(tree.Locate(1.5, 3));
			Assert.IsNull(tree.Locate(2.5, 0.9));
			Assert.IsNull(tree.Locate(2.5, 3));
		}

		[TestMethod]
		public void TestPlacementDegenerate()
		{
			// No two points share the same X position
			var mesh = new BndFactory(2).CreateMesh() as BndMesh;

			var face = BuildSquare(mesh);
			mesh.FinalizeMesh();

			var tree = Placement.GetPlacementTree(mesh);
			Assert.IsNotNull(tree.Locate(0.5, 0.5));
			Assert.IsNull(tree.Locate(-1, 0.5));
			Assert.IsNull(tree.Locate(0.5, -1));
			Assert.IsNull(tree.Locate(0.5, 3));
			Assert.IsNull(tree.Locate(1.5, 0.5));
		}
	}
}
