using MathNet.Numerics.LinearAlgebra;
using MeshNav;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNavTests
{
	class VtxWithNormals : Vertex
    { 
	    public Vector<T> NormalAccessor { get; set; }
	    public VtxWithNormals(Mesh mesh, Vector<T> vec) : base(mesh, vec) { }
	}

	class HalfEdgeFactoryWithNormals : Factory
	{
		public HalfEdgeFactoryWithNormals(int dimension) : base(dimension) { }

	    public override Vertex CreateVertex(Mesh mesh, Vector<T> vec)
		{
			return new VtxWithNormals(mesh, vec);
		}

	    public override Mesh CreateMesh()
	    {
	        return new MeshWithNormals(Dimension);
	    }
	}

    internal class MeshWithNormals : Mesh
	{
		public MeshWithNormals(int dimension) : base(dimension) {}

		protected override Factory GetFactory(int dimension)
		{
			return new HalfEdgeFactoryWithNormals(dimension);
		}
	}
}
