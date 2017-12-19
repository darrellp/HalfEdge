using System;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
    public class RayedVertex<T> : Vertex<T>, IAtInfinity where T : struct, IEquatable<T>, IFormattable
    {
        internal RayedVertex(Vector<T> vec, Mesh<T> mesh) : base(vec, mesh) { }
        public bool IsAtInfinity { get; set; }
    }
}
