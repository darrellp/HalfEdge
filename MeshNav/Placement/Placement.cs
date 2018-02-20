using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MeshNav.Placement
{
	public static class Placement
	{
		#region Placement Tree
		private static int _seed;

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Sets edge order. </summary>
		///
		/// <remarks>	Darrell Plank, 2/18/2018. </remarks>
		///
		/// <param name="seed">	The seed: -1 => mesh edge order, 0 => new random order, >0 => random seed. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public static void SetEdgeOrder(int seed)
		{
			_seed = seed;
		}

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

			IEnumerable<HalfEdge> edges = _seed == -1 ? mesh.Edges() : GetShuffledEdges(mesh);

			// TODO: We need to pass Rayed Faces so that placement may be extended to them.
			foreach (var edge in edges)
			{
				placementTree.AddEdge(edge);
			}

			placementTree.Finish();
			return placementTree;
		}
		#endregion

		#region Serialization
		public static void Serialize(PlacementTree tree, Func<Face, string> serializeFaceToString, TextWriter stm)
		{
			var serializer = new JsonSerializer
			{
				Context = new StreamingContext(StreamingContextStates.Other, (serializeFaceToString, new Dictionary<Face, string>())),
				NullValueHandling = NullValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Objects
			};

			using (JsonWriter writer = new JsonTextWriter(stm))
			{
				serializer.Serialize(writer, tree.Root);
			}
		}

		public static PlacementTree Deserialize(Func<string, object> serializeStringToObject, TextReader stm)
		{
			var serializer = new JsonSerializer
			{
				Context = new StreamingContext(StreamingContextStates.Other, (serializeStringToObject, new Dictionary<string, object>())),
				NullValueHandling = NullValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Objects
			};
			var ret = new PlacementTree();
			using (JsonReader reader = new JsonTextReader(stm))
			{
				ret.Root = serializer.Deserialize<PlacementNode>(reader);
			}

			return ret;
		}

		#endregion

		#region Utilities
		private static IEnumerable<HalfEdge> GetShuffledEdges(Mesh mesh)
		{
			var random = _seed == 0 ? new Random() : new Random(_seed);
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
