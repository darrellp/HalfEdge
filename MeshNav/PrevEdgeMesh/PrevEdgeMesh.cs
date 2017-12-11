using System;

namespace MeshNav.PrevEdgeMesh
{
    class PrevEdgeMesh<T> : Mesh<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Constructor
        public PrevEdgeMesh(int dimension) : base(dimension) { }
        #endregion

        #region Overrides
        protected override HalfEdgeFactory<T> GetFactory(int dimension)
        {
            return new PrevEdgeMeshFactory<T>(dimension);
        }
        #endregion
    }
}
