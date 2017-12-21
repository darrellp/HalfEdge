using System;
using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
	class RayedFace<T> : Face<T>, IBoundary where T : struct, IEquatable<T>, IFormattable
    {
        #region Public properties
        public bool IsBoundaryAccessor { get; set; }
        #endregion
    }
}
