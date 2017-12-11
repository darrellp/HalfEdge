using System;
using MathNet.Numerics.LinearAlgebra;
using MeshNav;
using MeshNav.BoundaryMesh;
using MeshNav.TraitInterfaces;

namespace MeshNavTests
{
	class SubClassedVertex<T> : Vertex<T> where T : struct, IEquatable<T>, IFormattable
	{
		public SubClassedVertex(Vector<T> position, Mesh<T> mesh) : base(position, mesh) { Console.WriteLine(); }
	}

	class SubClassedFace<T> : BoundaryMeshFace<T> where T : struct, IEquatable<T>, IFormattable
	{
		public SubClassedFace(){ }
	}

	class SubClassedHalfEdge<T> : HalfEdge<T> where T : struct, IEquatable<T>, IFormattable
	{
		public SubClassedHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
			: base(vertex, opposite, face, nextEdge) { }
	}

	class SubClassedHalfEdgeFactory<T> : BoundaryMeshFactory<T> where T : struct, IEquatable<T>, IFormattable
	{
		public SubClassedHalfEdgeFactory(int dimension) : base(dimension) { }

		public override Face<T> CreateFace()
		{
			return new SubClassedFace<T>();
		}

		public override HalfEdge<T> CreateHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
		{
			return new SubClassedHalfEdge<T>(vertex, opposite, face, nextEdge);
		}

		public override Vertex<T> CreateVertex(Mesh<T> mesh, Vector<T> vec)
		{
			return new SubClassedVertex<T>(vec, mesh);
		}
	}

	class SubClassedMesh<T> : BoundaryMesh<T> where T : struct, IEquatable<T>, IFormattable
	{
		public SubClassedMesh(int dimension) : base(dimension) {}

		protected override HalfEdgeFactory<T> GetFactory(int dimension)
		{
			return new SubClassedHalfEdgeFactory<T>(dimension);
		}
	}
}
