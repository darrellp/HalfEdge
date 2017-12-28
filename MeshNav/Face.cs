using System.Collections.Generic;
using System.Linq;
using MeshNav.TraitInterfaces;

namespace MeshNav
{
    public class Face
    {
        #region Public Properties
        public HalfEdge HalfEdge { get; internal set; }
	    public Mesh Mesh => HalfEdge.Mesh;
        #endregion

        #region Traits
        public bool IsBoundary
        {
            // ReSharper disable PossibleNullReferenceException
            get => !Mesh.BoundaryTrait && (this as IBoundary).IsBoundaryAccessor;
            set
            {
                if (Mesh.BoundaryTrait)
                {
                    (this as IBoundary).IsBoundaryAccessor = value;
                }
            }
            // ReSharper restore PossibleNullReferenceException
        }
        #endregion

        #region Constructor
        internal Face() {}
        #endregion

        #region Accessors
        public IEnumerable<HalfEdge> Edges()
        {
            if (HalfEdge == null)
            {
                // This occurs on the BoundaryFace when there are no border edges
                yield break;
            }
            var curEdge = HalfEdge;
            do
            {
                yield return curEdge;
                curEdge = curEdge.NextEdge;
            } while (curEdge != HalfEdge);
        }

        public IEnumerable<Vertex> Vertices()
        {
            return Edges().Select(e => e.InitVertex);
        }
        #endregion

        #region Validation
        internal bool Validate()
        {
            var edges = new HashSet<HalfEdge>();
            var lastEdge = (HalfEdge)null;
            foreach (var halfEdge in Edges())
            {
                if (edges.Contains(halfEdge))
                {
                    throw new MeshNavException("Edges loop back on themselves");
                }
                if (lastEdge != null && lastEdge.NextEdge != halfEdge)
                {
                    throw new MeshNavException("Improper edge linkage within face");
                }
                lastEdge = halfEdge;
                edges.Add(halfEdge);
            }
            return true;
        }
        #endregion
    }
}