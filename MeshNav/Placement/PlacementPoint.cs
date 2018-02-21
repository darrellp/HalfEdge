﻿using System.Runtime.Serialization;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	[DataContract]
	public class PlacementPoint
	{
		[DataMember]
		public T X { get; set; }
		[DataMember]
		public T Y { get; set; }

		public PlacementPoint() { }

		public PlacementPoint(Vertex vtx)
		{
			X = vtx.X;
			Y = vtx.Y;
		}

		public PlacementPoint(Vector vec)
		{
			X = vec.X;
			Y = vec.Y;
		}

		public Vector ToVector()
		{
			return new Vector(X, Y);
		}
	}
}
