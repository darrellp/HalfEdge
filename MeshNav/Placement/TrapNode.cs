using System;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	internal class TrapNode : PlacementNode
	{
		public  object Tag { get; set; }

		public TrapNode(Trapezoid trapezoid = null) : base(null, null, trapezoid)
		{
			trapezoid.Node = this;
		}

		internal override bool ShouldTravelLeft(T x, T y)
		{
			throw new NotImplementedException();
		}

		internal override bool ShouldTravelLeft(T x, T y, double edgeSlope)
		{
			throw new NotImplementedException();
		}
	}
}
