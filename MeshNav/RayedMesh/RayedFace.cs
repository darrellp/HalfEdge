using MeshNav.TraitInterfaces;

namespace MeshNav.RayedMesh
{
	class RayedFace : Face, IBoundary
    {
        #region Public properties
        public bool IsBoundaryAccessor { get; set; }
        #endregion
    }
}
