using System.Runtime.Serialization;
using Newtonsoft.Json;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	[DataContract]
	class YNode : PlacementNode
	{
		[DataMember]
		[JsonProperty(TypeNameHandling = TypeNameHandling.None)]
		private readonly PlacementPoint _leftEnd;
		[DataMember]
		[JsonProperty(TypeNameHandling = TypeNameHandling.None)]
		private readonly PlacementPoint _rightEnd;

		public YNode() { }

		// We don't actually need the slope to be serialized/deserialized.  It's only used in the
		// construction where we have to insert edges.
		private readonly T _slope;

		internal YNode(PlacementPoint leftEnd, PlacementPoint rightEnd, PlacementNode left, PlacementNode right) : base(left, right)
		{
			_leftEnd = leftEnd;
			_rightEnd = rightEnd;

			// PlacementTree.AddEdge() ensures that we use the half edge which goes from left to right
			// so we don't have to check that here.
			_slope = Slope(leftEnd.X, leftEnd.Y, rightEnd.X, rightEnd.Y);
			left.Parents.Add(this);
			right.Parents.Add(this);
		}

		// In the tree, left is "above", right is "below"
		internal override bool ShouldTravelLeft(T x, T y)
		{
			// We want to go left if we're above - i.e., to the left of the line travelling
			// from left to right.
			return Geometry2D.FLeft(_leftEnd.ToVector(), _rightEnd.ToVector(), Make(x, y));
		}

		// If we're adding a segment to the tree then we make the decision partially based on slopes
		internal override bool ShouldTravelLeft(T x, T y, T edgeSlope)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (y == _leftEnd.Y)
			{
				// We share our left endpoints so we compare slopes rather than above/below
				return edgeSlope > _slope;
			}
			else
			{
				return ShouldTravelLeft(x, y);
			}
		}

		internal YNode(PlacementNode left, PlacementNode right, Trapezoid trapezoid = null) : base(left, right, trapezoid)
		{
			left.Parents.Add(this);
			right.Parents.Add(this);
		}
	}
}
