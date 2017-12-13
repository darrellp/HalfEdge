using System;
using MeshNav.BoundaryMesh;

namespace MeshNav.PrevEdgeMesh
{
    class PrevEdgeMeshFactory<T> : HalfEdgeFactory<T> where T : struct, IEquatable<T>, IFormattable
    {
        public PrevEdgeMeshFactory(int dimension) : base(dimension) { }

        public override HalfEdge<T> CreateHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
        {
            return new PrevEdgeHalfEdge<T>(vertex, opposite, face, nextEdge);
        }

	    public override Mesh<T> CreateMesh()
	    {
		    return new PrevEdgeMesh<T>(Dimension);
	    }
    }
}
