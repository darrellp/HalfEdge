using System;
using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
    class RayedFace<T> : Face<T>, IAtInfinity where T : struct, IEquatable<T>, IFormattable
    {
        public bool IsAtInfinity { get; set; }
    }
}
