using System;
using MeshNav.TraitInterfaces;

namespace MeshNav
{
    public class HalfEdge<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Public Properties
        public Vertex<T> InitVertex { get; }                // InitVertex at the end of the half-edge
        public HalfEdge<T> Opposite { get; internal set; }  // Half edge in opposite direction
        public Face<T> Face { get; internal set; }          // Face the half edge borders
        public HalfEdge<T> NextEdge { get; internal set; }  // Next half edge around the face
	    public Mesh<T> Mesh => InitVertex.Mesh;
        #endregion

        #region Traits

        public HalfEdge<T> PreviousEdge
        {
            // ReSharper disable PossibleNullReferenceException
            // ReSharper disable SuspiciousTypeConversion.Global
            get => Mesh.PreviousEdgeTrait ? FindPreviousEdge() : (this as IPreviousEdge<T>).PreviousEdgeAccessor;
            set
            {
                if (Mesh.PreviousEdgeTrait)
                {
                    (this as IPreviousEdge<T>).PreviousEdgeAccessor = value;
                }
            }
            // ReSharper restore SuspiciousTypeConversion.Global
            // ReSharper restore PossibleNullReferenceException
       }

        private HalfEdge<T> FindPreviousEdge()
        {
            // We've got to have at least three edges to a face
            var curEdge = NextEdge.NextEdge;
            while (curEdge.NextEdge != this)
            {
                curEdge = curEdge.NextEdge;
            }
            return curEdge;
        }

        #endregion

        #region Constructor
        internal HalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
        {
            if (vertex != null)
            {
                InitVertex = vertex;
                vertex.Edge = this;
            }
            Opposite = opposite;
            Face = face;
            NextEdge = nextEdge;
        }
		internal HalfEdge(){}
        #endregion

        #region Accessors
        public Vertex<T> NextVertex => Opposite/*NextEdge*/.InitVertex;
        public Face<T> OppositeFace => Opposite.Face;
        #endregion
    }
}
