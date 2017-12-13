using System;

namespace MeshNav.BoundaryMesh
{
    internal class BoundaryMeshFactory<T> : HalfEdgeFactory<T> where T : struct, IEquatable<T>, IFormattable
    {
        public BoundaryMeshFactory(int dimension) : base(dimension) { }

        public override Face<T> CreateFace()
        {
            return new BoundaryMeshFace<T>();
        }

	    public override Mesh<T> CreateMesh()
	    {
		    return new BoundaryMesh<T>(Dimension);
	    }
    }

}
