using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MeshNav.RayedMesh
{
    class RayedHalfEdgeFactory<T> : HalfEdgeFactory<T> where T : struct, IEquatable<T>, IFormattable
    {
        public RayedHalfEdgeFactory(int dimension) : base(dimension) { }

        public override Face<T> CreateFace()
        {
            return new RayedFace<T>();
        }

        public override HalfEdge<T> CreateHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
        {
            return new RayedHalfEdge<T>(vertex, opposite, face, nextEdge);
        }

        public override Vertex<T> CreateVertex(Mesh<T> mesh, Vector<T> vec)
        {
            return new RayedVertex<T>(vec, mesh);
        }
    }
}
