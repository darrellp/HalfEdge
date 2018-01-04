using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
	public class RayedFace : Face, IBoundary
    {
        #region Public properties
        public bool IsBoundaryAccessor { get; set; }
        public bool IsOuterBoundary => IsBoundaryAccessor;
        #endregion
    }
}
