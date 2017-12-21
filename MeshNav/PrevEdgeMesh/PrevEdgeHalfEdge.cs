using System;
using MeshNav.TraitInterfaces;

namespace MeshNav.PrevEdgeMesh
{
    class PrevEdgeHalfEdge<T> : HalfEdge<T>, IPreviousEdge<T> where T : struct, IEquatable<T>, IFormattable
    {
        public PrevEdgeHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
            : base(vertex, opposite, face, nextEdge) { }
        public HalfEdge<T> PreviousEdgeAccessor { get; set; }
    }
}
