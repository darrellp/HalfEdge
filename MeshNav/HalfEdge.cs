using MeshNav.TraitInterfaces;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
    public class HalfEdge : MeshComponent
    {
        #region Public Properties
        public Vertex InitVertex { get; }                // InitVertex at the end of the half-edge
        public HalfEdge Opposite { get; internal set; }  // Half edge in opposite direction
        public Face Face { get; internal set; }          // Face the half edge borders
        public HalfEdge NextEdge { get; internal set; }  // Next half edge around the face
	    public Mesh Mesh => InitVertex.Mesh;
        #endregion

        #region DEBUGGING
#if DEBUG
        private static int _idNext;
        private readonly int _id;
#endif
#endregion

        #region Traits

        public HalfEdge PreviousEdge
        {
            // ReSharper disable PossibleNullReferenceException
            // ReSharper disable SuspiciousTypeConversion.Global
            get => Mesh.PreviousEdgeTrait ? (this as IPreviousEdge).PreviousEdgeAccessor : FindPreviousEdge();
            set
            {
                if (Mesh.PreviousEdgeTrait)
                {
                    (this as IPreviousEdge).PreviousEdgeAccessor = value;
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

	    public bool IsAtInfinity
	    {
		    get
		    {
			    if (!Mesh.RayedTrait)
			    {
				    return false;
			    }
			    // ReSharper disable PossibleNullReferenceException
			    return (InitVertex as IRayed).IsRayed && (NextVertex as IRayed).IsRayed;
			    // ReSharper restore PossibleNullReferenceException
		    }
	    }

	    public bool IsInboundRay
		{
		    get
		    {
			    if (!Mesh.RayedTrait)
			    {
				    return false;
			    }
			    // ReSharper disable PossibleNullReferenceException
			    return (InitVertex as IRayed).IsRayed && !(NextVertex as IRayed).IsRayed;
			    // ReSharper restore PossibleNullReferenceException
		    }
	    }

	    public bool IsOutboundRay
		{
		    get
		    {
			    if (!Mesh.RayedTrait)
			    {
				    return false;
			    }
			    // ReSharper disable PossibleNullReferenceException
			    return !(InitVertex as IRayed).IsRayed && (NextVertex as IRayed).IsRayed;
				// ReSharper restore PossibleNullReferenceException
			}
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
#if DEBUG
            _id = _idNext++;
#endif
        }
		internal HalfEdge(){}
        #endregion

        #region Accessors
        public Vertex NextVertex => Opposite/*NextEdge*/.InitVertex;
        public Face OppositeFace => Opposite.Face;
        #endregion

        #region Overrides

        public override string ToString()
        {
            // ReSharper disable once RedundantAssignment
            var tag = string.Empty;
#if DEBUG
            tag = $" - {_id}";
#endif
            return $"{InitVertex} - {NextVertex}{tag}";
        }

        #endregion
    }
}
