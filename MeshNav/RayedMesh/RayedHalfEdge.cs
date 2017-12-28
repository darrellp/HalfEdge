namespace MeshNav.RayedMesh
{
    public class RayedHalfEdge : HalfEdge
    {
        public RayedHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
            : base(vertex, opposite, face, nextEdge)
        {
        }

	    // ReSharper disable PossibleNullReferenceException
	    public bool IsAtInfinity => (InitVertex as RayedVertex).IsRayed && (NextVertex as RayedVertex).IsRayed;
	    public bool IsInboundRay => (InitVertex as RayedVertex).IsRayed && !(NextVertex as RayedVertex).IsRayed;
	    public bool IsOutboundRay => !(InitVertex as RayedVertex).IsRayed && (NextVertex as RayedVertex).IsRayed;
	    // ReSharper restore PossibleNullReferenceException
	}
}
