using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	[DataContract]
	internal class TrapNode : PlacementNode
	{
		[DataMember]
		public string TagString { get; set; }
		public  object Tag { get; set; }
		
		public TrapNode() { }

		public TrapNode(Trapezoid trapezoid = null) : base(null, null, trapezoid)
		{
			// ReSharper disable once PossibleNullReferenceException
			trapezoid.Node = this;
		}

		internal override bool IsLeaf()
		{
			return true;
		}

		internal override bool ShouldTravelLeft(T x, T y)
		{
			throw new NotImplementedException();
		}

		internal override bool ShouldTravelLeft(T x, T y, T edgeSlope)
		{
			throw new NotImplementedException();
		}

		private readonly Face _nullFace = new Face();

		[OnSerializing]
		internal void OnSerializingMethod(StreamingContext context)
		{
			(var serializeFaceToString, var faceDict) = ((Func<Face, string>, Dictionary<Face, string>))context.Context;
			var face = Trapezoid.ContainingFace;
			var lookup = face ?? _nullFace;
			if (!faceDict.ContainsKey(lookup))
			{
				faceDict[lookup] = serializeFaceToString(face);
			}

			TagString = faceDict[lookup];
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			(var serializeStringToObject, var stringDict) = ((Func<string, object>, Dictionary<string, object>)) context.Context;
			if (!stringDict.ContainsKey(TagString))
			{
				stringDict[TagString] = serializeStringToObject(TagString);
			}

			Tag = stringDict[TagString];
			TagString = null;
		}
	}
}
