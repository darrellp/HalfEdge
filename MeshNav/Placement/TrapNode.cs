using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MeshNav.Placement
{
	internal class TrapNode : PlacementNode
	{
		public TrapNode(Trapezoid trapezoid) : base(null, null, trapezoid)
		{
			trapezoid.Node = this;
		}

		internal override bool ShouldTravelLeft(Vector<double> queryPt)
		{
			throw new NotImplementedException();
		}

		internal override bool ShouldTravelLeft(Vector<double> queryPt, double edgeSlope)
		{
			throw new NotImplementedException();
		}
	}
}
