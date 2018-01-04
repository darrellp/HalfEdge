using MeshNav.TraitInterfaces;

namespace MeshNav.BoundaryMesh
{
    class BoundaryFace : Face, IBoundary
    {
        #region Public Properties
        // This distinguishes between an outer boundary and a boundary which represents an internal hole.  This
        // distinction only makes sense in 2D.
        public bool IsOuterBoundary
        {
            get
            {
                if (Mesh.Factory.Dimension != 2)
                {
                    throw new MeshNavException("Calling IsOuterBoundary in non-planar mesh isn't allowed");
                }
                if (!IsBoundaryAccessor)
                {
                    return false;
                }
                return ICcw() != HalfEdge.OppositeFace.ICcw();
            }
        }
        public bool IsBoundaryAccessor { get; set; }
        #endregion
    }
}
