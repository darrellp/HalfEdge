using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using MeshNav.Voronoi;
using Templates;

namespace MeshNavTests
{
	[TestClass]
	public class TestVoronoi
	{
		[TestMethod]
		public void TestSimpleCase()
		{
			var factory = new VoronoiFactory(2);
			var mesh = BuildHalfEdge.CreateVoronoi(new List<Vector>()
			{
				new Vector(1, 0),
				new Vector(-1, 0),
				new Vector(0, 1)
			},
				new VoronoiFactory(2));
			Assert.AreEqual(12, mesh.HalfEdges.Count());
			Assert.AreEqual(4, mesh.Vertices.Count());
			Assert.AreEqual(4, mesh.Faces.Count());
		}
	}
}
