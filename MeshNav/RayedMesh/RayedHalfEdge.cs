using System;

namespace MeshNav.RayedMesh
{
    public class RayedHalfEdge<T> : HalfEdge<T> where T : struct, IEquatable<T>, IFormattable
    {
        public RayedHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
            : base(vertex, opposite, face, nextEdge)
        {
        }

	    // ReSharper disable PossibleNullReferenceException
	    public bool IsAtInfinity => (InitVertex as RayedVertex<T>).IsRayed && (NextVertex as RayedVertex<T>).IsRayed;
	    public bool IsInboundRay => (InitVertex as RayedVertex<T>).IsRayed && !(NextVertex as RayedVertex<T>).IsRayed;
	    public bool IsOutboundRay => !(InitVertex as RayedVertex<T>).IsRayed && (NextVertex as RayedVertex<T>).IsRayed;
	    // ReSharper restore PossibleNullReferenceException
	}
}
