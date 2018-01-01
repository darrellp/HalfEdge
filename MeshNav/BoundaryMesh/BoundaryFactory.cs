using System.Collections.Generic;

namespace MeshNav.BoundaryMesh
{
    internal class BoundaryFactory : Factory
    {
        public BoundaryFactory(int dimension) : base(dimension) { }

        public override Face CreateFace()
        {
            return new BoundaryFace();
        }

        protected internal override void CloneFace(Face newFace, Face face, Dictionary<HalfEdge, HalfEdge> oldToNewHalfEdge, Dictionary<Vertex, Vertex> oldToNewVertex)
        {
            base.CloneFace(newFace, face, oldToNewHalfEdge, oldToNewVertex);
            newFace.IsBoundary = face.IsBoundary;
        }
    }

}
