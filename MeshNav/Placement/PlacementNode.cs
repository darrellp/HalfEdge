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
	internal abstract class PlacementNode
	{
		internal PlacementNode Left { get; set; }
		internal PlacementNode Right { get; set; }
		internal List<PlacementNode> Parents { get; set; }
		internal Trapezoid Trapezoid { get; }

		internal bool IsTrapNode => Trapezoid == null;

		protected PlacementNode(PlacementNode left, PlacementNode right, Trapezoid trapezoid = null)
		{
			Left = left;
			Right = right;
			Trapezoid = trapezoid;
			Parents = new List<PlacementNode>();
		}

		internal abstract bool ShouldTravelLeft(T x, T y);
		internal abstract bool ShouldTravelLeft(T x, T y, T edgeSlope);

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
