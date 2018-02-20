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
	internal class XNode : PlacementNode
	{
		[DataMember]
		[JsonProperty(TypeNameHandling = TypeNameHandling.None)]
		private readonly PlacementPoint _point;

		public XNode() { }

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
