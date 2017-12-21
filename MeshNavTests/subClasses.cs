using System;
using MathNet.Numerics.LinearAlgebra;
using MeshNav;
using MeshNav.TraitInterfaces;

namespace MeshNavTests
{
	class VtxWithNormals<T> : Vertex<T>, INormal<T> where T : struct, IEquatable<T>, IFormattable
	{
	    public Vector<T> NormalAccessor { get; set; }
	    public VtxWithNormals(Mesh<T> mesh, Vector<T> vec) : base(mesh, vec) { }
	}

	class HalfEdgeFactoryWithNormals<T> : Factory<T> where T : struct, IEquatable<T>, IFormattable
	{
		public HalfEdgeFactoryWithNormals(int dimension) : base(dimension) { }
		public override Vertex<T> CreateVertex(Mesh<T> mesh, Vector<T> vec)
		{
			return new VtxWithNormals<T>(mesh, vec);
		}

	    public override Mesh<T> CreateMesh()
	    {
	        return new MeshWithNormals<T>(Dimension);
	    }
	}

	class MeshWithNormals<T> : Mesh<T> where T : struct, IEquatable<T>, IFormattable
	{
		public MeshWithNormals(int dimension) : base(dimension) {}

		protected override Factory<T> GetFactory(int dimension)
		{
			return new HalfEdgeFactoryWithNormals<T>(dimension);
		}
	}
}
