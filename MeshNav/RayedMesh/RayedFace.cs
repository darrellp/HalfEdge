using System;
using System.Linq;
using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
	class RayedFace : Face, IBoundary
    {
        #region Public properties
        public bool IsBoundaryAccessor { get; set; }
        #endregion

        public override int ICcw()
        {
            // We just skip the rayed vertices
            return Math.Sign(Geometry2D.SignedArea(Vertices().Where(v => !(v as IRayed).IsRayed).Select(v => v.Position).ToList()));
        }
    }
}
