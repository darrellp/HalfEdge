using System;
using System.Collections.Generic;
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
		private readonly PlacementPoint _point;

		internal override bool ShouldTravelLeft(T x, T y)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return x == _point.X ? y < _point.Y : x < _point.X;
		}

		internal override bool ShouldTravelLeft(T x, T y, T edgeSlope)
		{
			return ShouldTravelLeft(x, y);
		}

		public XNode(PlacementNode left, PlacementNode right, Vertex vtx) : base(left, right)
		{
			_point = new PlacementPoint(vtx);
			left.Parents.Add(this);
			right.Parents.Add(this);
		}
	}
}
