using System.Collections.Generic;
using static MeshNav.Utilities;
using static MeshNav.Geometry2D;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	public class PlacementTree
	{
		private TrapezoidalMap _map;
		internal PlacementNode Root { get; set; }
		internal bool FromDeserialized { get; set; }

		public PlacementTree()
		{
			_map = new TrapezoidalMap();
		}

		internal void AddEdge(HalfEdge edge)
		{
			// Make sure the half edge being added goes from left to right.
			if (edge.InitVertex.X > edge.NextVertex.X)
			{
				edge = edge.Opposite;
			}

			Trapezoid lNeighborTop = null, lNeighborBottom = null;

			foreach (var trap in LocateTrapezoids(edge))
			{
				PlacementNode repl;
				if (trap.Left <= edge.InitVertex.X || trap.Right >= edge.NextVertex.X)
				{
					repl = _map.UpdateEndTrapezoid(trap, edge, ref lNeighborTop, ref lNeighborBottom);
				}
				else
				{
					repl = _map.UpdateMiddleTrapezoid(trap, edge, ref lNeighborTop, ref lNeighborBottom);
				}

				foreach (var parent in trap.Node.Parents)
				{
					parent.ReplaceSon(trap.Node, repl);
				}

				if (Root == null)
				{
					Root = repl;
				}

			}
		}

		private TrapNode LocateNode(T x, T y)
		{
			var curNode = Root;
			while (!curNode.IsLeaf())
			{
				curNode = curNode.ShouldTravelLeft(x, y) ? curNode.Left : curNode.Right;
			}

			return (TrapNode) curNode;
		}

		public object Locate(T x, T y)
		{
			// TODO: Should we somehow allow for Tags in non-deserialized trees?
			// The only place we currently specify how to turn a Face into a tag is in the callback
			// functions passed down to Serialize/Deserialize so they would have to somehow be included
			// in the creation phase also which seems weird.  Perhaps we could make a tag trait for
			// faces and use it as the tag in the TrapezoidNode?
			if (!FromDeserialized)
			{
				throw new MeshNavException("Locate() is not available for unserialized trees");
			}
			return LocateNode(x, y).Tag;
		}

		public Face LocateFace(T x, T y)
		{
			if (FromDeserialized)
			{
				throw new MeshNavException("LocateFace() is not available for deserialized placement trees");
			}
			return LocateTrapezoid(x, y).ContainingFace;
		}

		private Trapezoid LocateTrapezoid(T x, T y)
		{
			return LocateNode(x, y).Trapezoid;
		}

		private Trapezoid LocateTrapezoid(HalfEdge edge, T slope)
		{
			var curNode = Root;
			var queryPt = edge.InitVertex.Position;
			var x = queryPt.X;
			var y = queryPt.Y;

			while (!curNode.IsLeaf())
			{
				curNode = curNode.ShouldTravelLeft(x, y, slope) ? curNode.Left : curNode.Right;
			}
			return curNode.Trapezoid;
		}

		private List<Trapezoid> LocateTrapezoids(HalfEdge edge)
		{
			if (Root == null)
			{
				return new List<Trapezoid> {new TrapNode(_map.Bbox).Trapezoid};
			}
			var ret = new List<Trapezoid>();
			var slope = Slope(edge.InitVertex.Position, edge.NextVertex.Position);
			var tCur = LocateTrapezoid(edge, slope);
			var edgeRight = edge.NextVertex.X;
			ret.Add(tCur);
			while (tCur.RightVtx.X < edgeRight)
			{
				tCur = NextTrapezoid(tCur, edge);
				ret.Add(tCur);
			}
			return ret;
		}

		private Trapezoid NextTrapezoid(Trapezoid tCur, HalfEdge edge)
		{
			if (FLeft(edge.InitVertex.Position, edge.NextVertex.Position, tCur.RightVtx.Position))
			{
				// We're below - take lower right exit
				return tCur.RightBottom;
			}

			// We're above the right point so take the upper right exit
			return tCur.RightTop;
		}

		internal void Finish()
		{
			// dereference map so it can be garbage collected
			_map = null;
		}
	}
}
