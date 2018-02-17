using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.TraitInterfaces;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	class XNode : PlacementNode
	{
		private T _value;

		internal override bool ShouldTravelLeft(Vector<T> queryPt)
		{
			return queryPt.X() < _value;
		}

		internal override bool ShouldTravelLeft(Vector<T> queryPt, T edgeSlope)
		{
			return queryPt.X() < _value;
		}

		public XNode(PlacementNode left, PlacementNode right, T value) : base(left, right)
		{
			_value = value;
			left.Parents.Add(this);
			right.Parents.Add(this);
		}
	}
}
