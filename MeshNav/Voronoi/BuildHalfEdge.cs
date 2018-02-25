using System.Collections.Generic;
using System.Linq;
using DAP.CompGeom;
using MeshNav.RayedMeshSpace;
using MeshNav.TraitInterfaces;

namespace MeshNav.Voronoi
{
	internal class BuildHalfEdge
	{
		public static Mesh CreateVoronoi(List<Vector> points, Factory factory)
		{
			var fortune = new Fortune(points);
			fortune.Voronoi();
			return PopulateMesh(fortune, factory);
		}

		internal static Mesh PopulateMesh(Fortune fortune, Factory factory)
		{
			var mesh = factory.CreateMesh() as RayedMesh;
			// ReSharper disable once PossibleNullReferenceException
			if (mesh.Dimension != 2)
			{
				throw new MeshNavException("Voronoi requires 2D mesh");
			}

			if (!mesh.VoronoiTrait)
			{
				throw new MeshNavException("Voronoi requires rayed mesh");
			}

			foreach (var poly in fortune.Polygons)
			{
				ProcessPolygonEdges(poly, mesh);
			}

			return mesh;
		}

		private static void ProcessPolygonEdges(FortunePoly poly, RayedMesh mesh)
		{
			poly.SortEdges();
			var edges = AddEdgeAtInfinity(poly);
			WeVertex vtxNext;

			// If this poly is a rayed polygon then we need an extra spot in the vertex array for the
			// edge at infinity which HalfEdge needs but isn't present in the FortunePolygon.
			var polyVertices = new Vertex[edges.Count];

			// We have to prime the pump here
			if (edges[0].VtxStart == edges[1].VtxStart ||
			    edges[0].VtxStart == edges[1].VtxEnd)
			{
				vtxNext = edges[0].VtxStart;
				polyVertices[0] = AddVertex(edges[0].VtxEnd, mesh);
			}
			else
			{
				vtxNext = edges[0].VtxEnd;
				polyVertices[0] = AddVertex(edges[0].VtxStart, mesh);
			}

			// We skip the first edge since it's already been added in the pump priming above
			for (var ivtx = 1; ivtx < edges.Count; ivtx++)
			{
				var edge = edges[ivtx];

				// Add in the leading vertex for this edge
				polyVertices[ivtx] = AddVertex(vtxNext, mesh);
				if (!edge.FZeroLength())
				{
					// What about when we traverse this edge from the other direction?
					// We don't want to add one vertex going CW and another going CCW.
					// I think maybe we have to check if the end or start has already
					// been inserted and use that vertex if so.  Still - won't that fail
					// if there are more than one zero length edge converging?
					// TODO: think about this
					vtxNext = edge.VtxStart == vtxNext ? edge.VtxEnd : edge.VtxStart;
				}
			}

			// Now add the Polygon
			var newFace = mesh.AddFace(polyVertices);
			// ReSharper disable once PossibleNullReferenceException
			// ReSharper disable once SuspiciousTypeConversion.Global
			(newFace as IVoronoi).VoronoiPoint = poly.VoronoiPoint;
		}

		private static List<FortuneEdge> AddEdgeAtInfinity(FortunePoly poly)
		{
			var newList = new List<FortuneEdge>(poly.FortuneEdges.Count + 1);
			int iEdge;
			for (iEdge = 0; iEdge < poly.FortuneEdges.Count; iEdge++)
			{
				newList.Add(poly.FortuneEdges[iEdge]);
				if (poly.FortuneEdges[iEdge].FRay)
				{
					var curEdge = poly.FortuneEdges[iEdge];
					var iNext = (iEdge + 1) % poly.FortuneEdges.Count;
					var nextEdge = poly.FortuneEdges[iNext];
					if (nextEdge.FRay)
					{
						var newEdge = new FortuneEdge
						{
							VtxStart = curEdge.VtxEnd,
							VtxEnd = nextEdge.VtxEnd
						};
						newList.Add(newEdge);
						break;
					}
				}
			}

			newList.AddRange(poly.FortuneEdges.Skip(iEdge + 1));
			return newList;
		}

		private static Vertex AddVertex(WeVertex vtx, RayedMesh mesh)
		{
			// ReSharper disable once ConvertIfStatementToNullCoalescingExpression
			if (vtx.HalfEdgeVertex == null)
			{
				vtx.HalfEdgeVertex = vtx.FAtInfinity ? mesh.AddRayedVertex(vtx.Pt) : mesh.AddVertex(vtx.Pt);
			}

			return vtx.HalfEdgeVertex;
		}
	}
}