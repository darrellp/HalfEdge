using System;
using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
    class RayedHalfEdge<T> : HalfEdge<T>, IRayed, IAtInfinity where T : struct, IEquatable<T>, IFormattable
    {
        public RayedHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
            : base(vertex, opposite, face, nextEdge)
        {
        }

        public bool IsRayed { get; internal set; }
        public bool IsAtInfinity { get; internal set; }
    }
}
