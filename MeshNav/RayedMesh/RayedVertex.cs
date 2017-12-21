using System;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
    public class RayedVertex<T> : Vertex<T>, IRayed where T : struct, IEquatable<T>, IFormattable
    {
        internal RayedVertex(Mesh<T> mesh, Vector<T> vec) : base(mesh, vec) { }
        public bool IsRayed { get; set; }
    }
}
