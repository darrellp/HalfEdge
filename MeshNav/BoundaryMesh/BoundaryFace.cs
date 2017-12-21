using System;
using MeshNav.TraitInterfaces;

namespace MeshNav.BoundaryMesh
{
    class BoundaryFace<T> : Face<T>, IBoundary where T : struct, IEquatable<T>, IFormattable
    {
        #region Public Properties
        public bool IsBoundaryAccessor { get; set; }
        #endregion
    }
}
