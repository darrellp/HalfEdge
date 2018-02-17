using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	internal abstract class PlacementNode
	{
		internal PlacementNode Left { get; set; }
		internal PlacementNode Right { get; set; }
		internal List<PlacementNode> Parents { get; set; }
		internal Trapezoid Trapezoid { get; private set; }

		internal bool IsTrapNode => Trapezoid == null;

		protected PlacementNode(PlacementNode left, PlacementNode right, Trapezoid trapezoid = null)
		{
			Left = left;
			Right = right;
			Trapezoid = trapezoid;
			Parents = new List<PlacementNode>();
		}

		internal PlacementNode NextNode(Vector<T> queryPt)
		{
			return ShouldTravelLeft(queryPt) ? Left : Right;
		}

		internal abstract bool ShouldTravelLeft(Vector<T> queryPt);
		internal abstract bool ShouldTravelLeft(Vector<T> queryPt, T edgeSlope);

		internal void ReplaceSon(PlacementNode oldSon, PlacementNode newSon)
		{
			if (Left == oldSon)
			{
				Left = newSon;
			}
			else
			{
				Right = newSon;
			}
		}
	}
}
