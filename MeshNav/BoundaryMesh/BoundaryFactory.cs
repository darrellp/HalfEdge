using System;

namespace MeshNav.BoundaryMesh
{
    internal class BoundaryFactory<T> : Factory<T> where T : struct, IEquatable<T>, IFormattable
    {
        public BoundaryFactory(int dimension) : base(dimension) { }

        public override Face<T> CreateFace()
        {
            return new BoundaryFace<T>();
        }
    }

}
