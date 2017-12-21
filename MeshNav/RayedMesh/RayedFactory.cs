using System;
using MathNet.Numerics.LinearAlgebra;

namespace MeshNav.RayedMesh
{
    public class RayedFactory<T> : Factory<T> where T : struct, IEquatable<T>, IFormattable
    {
        public RayedFactory(int dimension) : base(dimension) { }

        public override HalfEdge<T> CreateHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
        {
            return new RayedHalfEdge<T>(vertex, opposite, face, nextEdge);
        }

        public override Vertex<T> CreateVertex(Mesh<T> mesh, Vector<T> vec)
        {
            return new RayedVertex<T>(mesh, vec);
        }

        public override Face<T> CreateFace()
        {
            return new RayedFace<T>();
        }
    }
}
