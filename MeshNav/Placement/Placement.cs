using System;
using System.Collections.Generic;
using System.Linq;

namespace MeshNav.Placement
{
	public static class Placement
	{
		#region Placement Tree
		public static PlacementTree GetPlacementTree(Mesh mesh)
		{
			if (!mesh.IsInitialized)
			{
				throw new ArgumentException($"Mesh not initialized in {nameof(GetPlacementTree)}");
			}
			if (mesh.Dimension != 2)
			{
				throw new ArgumentException($"Mesh not 2D in {nameof(GetPlacementTree)}");
			}

			if (mesh.Faces.Any(f => !f.IsSimple))
			{
				throw new ArgumentException($"Mesh contains non-simple polygons in {nameof(GetPlacementTree)}");
			}

			var placementTree = new PlacementTree();

			// TODO: We need to pass Rayed Faces so that placement may be extended to them.
			foreach (var edge in mesh.Edges())
			{
				placementTree.AddEdge(edge);
			}

			placementTree.Finish();
			return placementTree;
		}
		#endregion

		#region Utilities
		private static IEnumerable<HalfEdge> GetShuffledEdges(Mesh mesh)
		{
			var random = new Random();
			var edgeList = mesh.Edges().Where(e => !e.IsRayed).ToList();
			for (var i = 0; i < edgeList.Count; i++)
			{
				var swapIndex = random.Next(i, edgeList.Count - 1);
				(edgeList[i], edgeList[swapIndex]) = (edgeList[swapIndex], edgeList[i]);
				yield return edgeList[i];
			}
		}
		#endregion
	}
}
