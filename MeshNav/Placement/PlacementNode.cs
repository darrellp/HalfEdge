using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	[DataContract]
	internal abstract class PlacementNode
	{
		[DataMember]
		internal PlacementNode Left { get; set; }
		[DataMember]
		internal PlacementNode Right { get; set; }
		internal List<PlacementNode> Parents { get; set; }
		internal Trapezoid Trapezoid { get; }

		protected PlacementNode() { }

		protected PlacementNode(PlacementNode left, PlacementNode right, Trapezoid trapezoid = null)
		{
			Left = left;
			Right = right;
			Trapezoid = trapezoid;
			Parents = new List<PlacementNode>();
		}

		internal abstract bool ShouldTravelLeft(T x, T y);
		internal abstract bool ShouldTravelLeft(T x, T y, T edgeSlope);

		internal virtual bool IsLeaf()
		{
			return false;

		}

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
