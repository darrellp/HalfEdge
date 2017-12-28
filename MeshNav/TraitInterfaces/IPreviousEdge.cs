using System;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.TraitInterfaces
{
    public interface IPreviousEdge
    {
        // To be implemented by HalfEdges if we keep both forward and backward links for next and previous halfedges
        HalfEdge PreviousEdgeAccessor { get; set; }
    }
}
