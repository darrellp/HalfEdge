using MeshNav.TraitInterfaces;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
    public class HalfEdge
    {
        #region Public Properties
        public Vertex InitVertex { get; }                // InitVertex at the end of the half-edge
        public HalfEdge Opposite { get; internal set; }  // Half edge in opposite direction
        public Face Face { get; internal set; }          // Face the half edge borders
        public HalfEdge NextEdge { get; internal set; }  // Next half edge around the face
	    public Mesh Mesh => InitVertex.Mesh;
        #endregion

        #region Traits

        public HalfEdge PreviousEdge
        {
            // ReSharper disable PossibleNullReferenceException
            // ReSharper disable SuspiciousTypeConversion.Global
            get => Mesh.PreviousEdgeTrait ? (this as IPreviousEdge<T>).PreviousEdgeAccessor : FindPreviousEdge();
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

        private HalfEdge FindPreviousEdge()
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
        internal HalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
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
        public Vertex NextVertex => Opposite/*NextEdge*/.InitVertex;
        public Face OppositeFace => Opposite.Face;
        #endregion
    }
}
