using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.TraitInterfaces;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.RayedMesh
{
    public class RayedFactory : Factory
    {
        public RayedFactory(int dimension) : base(dimension) { }

        public override Vertex CreateVertex(Mesh mesh, Vector<T> vec)
        {
            return new RayedVertex(mesh, vec);
        }

        public override Face CreateFace()
        {
            return new RayedFace();
        }

        protected internal override void CloneFace(Face newFace, Face face, Dictionary<HalfEdge, HalfEdge> oldToNewHalfEdge, Dictionary<Vertex, Vertex> oldToNewVertex)
        {
            base.CloneFace(newFace, face, oldToNewHalfEdge, oldToNewVertex);
            newFace.IsBoundary = face.IsBoundary;
        }

        protected internal override void CloneVertex(Vertex newVertex, Vertex oldVertex)
        {
            base.CloneVertex(newVertex, oldVertex);
            // ReSharper disable PossibleNullReferenceException
            (newVertex as IRayed).IsRayed = (oldVertex as IRayed).IsRayed;
            // ReSharper restore PossibleNullReferenceException
        }
    }
}
