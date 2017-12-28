using MeshNav.TraitInterfaces;

namespace MeshNav.BoundaryMesh
{
    class BoundaryFace : Face, IBoundary
    {
        #region Public Properties
        public bool IsBoundaryAccessor { get; set; }
        #endregion
    }
}
