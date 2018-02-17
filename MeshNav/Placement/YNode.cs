using MathNet.Numerics.LinearAlgebra;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	class YNode : PlacementNode
	{
		private readonly Vector<T> _left;
		private readonly Vector<T> _right;
		private readonly T _slope;

		internal YNode(HalfEdge edge, PlacementNode left, PlacementNode right) : base(left, right)
		{
			_left = edge.InitVertex.Position;
			_right = edge.NextVertex.Position;

			// PlacementTree.AddEdge() ensures that we use the half edge which goes from left to right
			// so we don't have to check that here.
			_slope = Slope(_left, _right);
			left.Parents.Add(this);
			right.Parents.Add(this);
		}

		// In the tree, left is "above", right is "below"
		internal override bool ShouldTravelLeft(Vector<T> queryPt)
		{
			// We want to go left if we're above - i.e., to the left of the line travelling
			// from left to right.
			return Geometry2D.FLeft(_left, _right, queryPt);
		}

		// If we're adding a segment to the tree then we make the decision partially based on slopes
		internal override bool ShouldTravelLeft(Vector<T> queryPt, T edgeSlope)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (queryPt.Y() == _left.Y())
			{
				// We share our left endpoints so we compare slopes rather than above/below
				return edgeSlope > _slope;
			}
			else
			{
				return ShouldTravelLeft(queryPt);
			}
		}

		internal YNode(PlacementNode left, PlacementNode right, Trapezoid trapezoid = null) : base(left, right, trapezoid)
		{
			left.Parents.Add(this);
			right.Parents.Add(this);
		}
	}
}
