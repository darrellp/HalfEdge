﻿using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using MeshNav.Placement;
using Templates;

namespace MeshNavTests
{
	[TestClass]
	public class TestPlacement
	{
		private void BuildParallelogram(Mesh mesh)
		{
			// ReSharper disable InconsistentNaming
			var ptLL = mesh.AddVertex(0, 0);
			var ptLR = mesh.AddVertex(2, 0);
			var ptUR = mesh.AddVertex(3, 1);
			var ptUL = mesh.AddVertex(1, 1);
			// ReSharper restore InconsistentNaming
			mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
		}

		private void BuildTrapezoid(Mesh mesh)
		{
			// ReSharper disable InconsistentNaming
			var ptLL = mesh.AddVertex(0, 0);
			var ptLR = mesh.AddVertex(3, 0);
			var ptUR = mesh.AddVertex(2, 1);
			var ptUL = mesh.AddVertex(1, 1);
			// ReSharper restore InconsistentNaming
			mesh.AddFace(ptLL, ptUL, ptUR, ptLR);
		}

		[TestMethod]
		public void TestPlacementGeneralEndpointCase()
		{
			// Use mesh order for edges
			Placement.SetEdgeOrder(-1);

			// No two points share the same X position
			var mesh = new BndFactory(2).CreateMesh() as BndMesh;

			BuildParallelogram(mesh);
			// ReSharper disable once PossibleNullReferenceException
			mesh.FinalizeMesh();

			var tree = Placement.GetPlacementTree(mesh);
			Assert.IsNotNull(tree.LocateFace(0.5, 0.1));
			Assert.IsNotNull(tree.LocateFace(1.5, 0.1));
			Assert.IsNotNull(tree.LocateFace(2.5, 0.9));
			Assert.IsNull(tree.LocateFace(-1, 0));
			Assert.IsNull(tree.LocateFace(0.5, -1));
			Assert.IsNull(tree.LocateFace(0.5, 3));
			Assert.IsNull(tree.LocateFace(1.5, -1));
			Assert.IsNull(tree.LocateFace(1.5, 3));
			Assert.IsNull(tree.LocateFace(2.5, 0.1));
			Assert.IsNull(tree.LocateFace(2.5, 3));
		}

		[TestMethod]
		public void TestPlacementGeneralMiddleCase()
		{
			// Use mesh order for edges
			Placement.SetEdgeOrder(-1);

			// No two points share the same X position
			var mesh = new BndFactory(2).CreateMesh() as BndMesh;

			BuildTrapezoid(mesh);
			// ReSharper disable once PossibleNullReferenceException
			mesh.FinalizeMesh();

			var tree = Placement.GetPlacementTree(mesh);
			Assert.IsNotNull(tree.LocateFace(0.5, 0.1));
			Assert.IsNotNull(tree.LocateFace(1.5, 0.1));
			Assert.IsNotNull(tree.LocateFace(2.5, 0.1));
			Assert.IsNull(tree.LocateFace(-1, 0));
			Assert.IsNull(tree.LocateFace(0.5, -1));
			Assert.IsNull(tree.LocateFace(0.5, 3));
			Assert.IsNull(tree.LocateFace(1.5, -1));
			Assert.IsNull(tree.LocateFace(1.5, 3));
			Assert.IsNull(tree.LocateFace(2.5, 0.9));
			Assert.IsNull(tree.LocateFace(2.5, 3));
		}

		[TestMethod]
		public void TestPlacementDegenerate()
		{
			// Use mesh order for edges
			Placement.SetEdgeOrder(-1);

			var mesh = new BndFactory(2).CreateMesh() as BndMesh;

			// ReSharper disable PossibleNullReferenceException
			// ReSharper disable InconsistentNaming
			var ptLL = mesh.AddVertex(0, 0);
			var ptLR = mesh.AddVertex(1, 0);
			var ptUL = mesh.AddVertex(0, 1);
			var ptUR = mesh.AddVertex(1, 1);
			// ReSharper restore InconsistentNaming
			mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
			mesh.FinalizeMesh();

			var tree = Placement.GetPlacementTree(mesh);
			Assert.IsNotNull(tree.LocateFace(0.5, 0.5));
			Assert.IsNull(tree.LocateFace(-1, 0.5));
			Assert.IsNull(tree.LocateFace(0.5, -1));
			Assert.IsNull(tree.LocateFace(0.5, 3));
			Assert.IsNull(tree.LocateFace(1.5, 0.5));

			mesh = new BndFactory(2).CreateMesh() as BndMesh;
			ptLL = mesh.AddVertex(0, 0);
			ptLR = mesh.AddVertex(1, 0);
			ptUL = mesh.AddVertex(0, 1);
			ptUR = mesh.AddVertex(1, 1);

			var ul = mesh.AddFace(ptLL, ptUR, ptUL);
			var lr = mesh.AddFace(ptUR, ptLL, ptLR);
			mesh.FinalizeMesh();

			tree = Placement.GetPlacementTree(mesh);
			Assert.AreEqual(tree.LocateFace(0.5, 0.75), ul);
			Assert.AreEqual(tree.LocateFace(0.5, 0.25), lr);
			// ReSharper restore PossibleNullReferenceException
		}

		[TestMethod]
		public void TestPlacementSerialization()
		{
			var sb = new StringBuilder();
			var sw = new StringWriter(sb);

			// Use mesh order for edges
			Placement.SetEdgeOrder(-1);

			// No two points share the same X position
			var mesh = new BndFactory(2).CreateMesh() as BndMesh;

			BuildTrapezoid(mesh);
			// ReSharper disable once PossibleNullReferenceException
			mesh.FinalizeMesh();

			var tree = Placement.GetPlacementTree(mesh);
			Placement.Serialize(tree, f => f == null ? "outside" : "The one and only!", sw);

			StringReader sr = new StringReader(sb.ToString());
			tree = Placement.Deserialize(s => s, sr);
			Assert.AreEqual(tree.Locate(0.5, 0.1), "The one and only!");
			Assert.AreEqual(tree.Locate(1.5, 0.1), "The one and only!");
			Assert.AreEqual(tree.Locate(2.5, 0.1), "The one and only!");
			Assert.AreEqual(tree.Locate(-1, 0), "outside");
			Assert.AreEqual(tree.Locate(0.5, -1), "outside");
			Assert.AreEqual(tree.Locate(0.5, 3), "outside");
			Assert.AreEqual(tree.Locate(1.5, -1), "outside");
			Assert.AreEqual(tree.Locate(1.5, 3), "outside");
			Assert.AreEqual(tree.Locate(2.5, 0.9), "outside");
			Assert.AreEqual(tree.Locate(2.5, 3), "outside");
		}
	}
}
