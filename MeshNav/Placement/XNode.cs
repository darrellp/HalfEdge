using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	class XNode : PlacementNode
	{
		private readonly T _value;
		private readonly Vector<T> _point;

		internal override bool ShouldTravelLeft(Vector<T> queryPt)
		{
			var queryX = queryPt.X();
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return queryX == _value ? queryPt.Y() < _point.Y() : queryX < _value;
		}

		internal override bool ShouldTravelLeft(Vector<T> queryPt, T edgeSlope)
		{
			return ShouldTravelLeft(queryPt);
		}

		public XNode(PlacementNode left, PlacementNode right, Vertex vtx) : base(left, right)
		{
			_point = vtx.Position;
			_value = _point.X();
			left.Parents.Add(this);
			right.Parents.Add(this);
		}
	}
}
