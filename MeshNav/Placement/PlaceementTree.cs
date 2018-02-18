using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
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
		private PlacementNode _root;

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

				if (_root == null)
				{
					_root = repl;
				}

			}
		}

		public Face Locate(T x, T y)
		{
			return LocateTrapezoid(Make(x, y)).ContainingFace;
		}

		private Trapezoid LocateTrapezoid(Vector<T> queryPt)
		{
			var curNode = _root;
			while (curNode.Trapezoid == null)
			{
				curNode = curNode.ShouldTravelLeft(queryPt) ? curNode.Left : curNode.Right;
			}
			return curNode.Trapezoid;
		}

		private Trapezoid LocateTrapezoid(HalfEdge edge, T slope)
		{
			var curNode = _root;
			var queryPt = edge.InitVertex.Position;
			while (curNode.Trapezoid == null)
			{
				curNode = curNode.ShouldTravelLeft(queryPt, slope) ? curNode.Left : curNode.Right;
			}
			return curNode.Trapezoid;
		}

		private List<Trapezoid> LocateTrapezoids(HalfEdge edge)
		{
			if (_root == null)
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

		public void Finish()
		{
			// dereference map so it can be garbage collected
			_map = null;
		}
	}
}
