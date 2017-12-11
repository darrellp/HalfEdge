using System;
using MeshNav.TraitInterfaces;

namespace MeshNav.BoundaryMesh
{
    class BoundaryMeshFace<T> : Face<T>, IAtInfinity where T : struct, IEquatable<T>, IFormattable
    {
        #region Public Properties
        public bool AtInfinityAccessor { get; set; }
        #endregion
    }
}
