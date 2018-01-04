using System.Linq;
using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.RayedMesh
{
    public class RayedVertex : Vertex
    {
		// Arbitrary value that our rayed points are "moved out" by.  There are no "real"
		// points for rayed vertices - just infinite rays - so if we need a real point we
		// have to just move out some large distance from the fixed end of the ray.  It's
		// not a great solution but I'm not sure of a better one unless we somehow deal
		// directly with rays for some half edges and segments for others. That would affect
		// pretty much every piece of code that dealt with halfEdges.  MaxLength is a public
		// value so if it's too low clients can set it higher.  For calculation purposes
		// it's not that important.  If you'd like to "draw" then you want it large enough
		// that it gets clipped off the edge of your drawing surface.
        public T MaxLength = 1000000;

        internal RayedVertex(Mesh mesh, Vector<T> vec)
        {
            Mesh = mesh;
            Ray = vec;
        }

        public bool IsRayed { get; set; }

	    public Vector<T> Ray
	    {
		    get => base.Position;
		    set => base.Position = value;
	    }

	    public override Vector<T> Position
	    {
		    get
		    {
			    if (!IsRayed)
			    {
				    return base.Position;
			    }
			    var unrayed = AdjacentEdges().First(he => he.IsInboundRay).NextVertex;
			    return unrayed.Position + Ray * MaxLength;
		    }
		    set => throw new MeshNavException("Trying to set position of rayed vertex - use Mesh.AddRayedVertex or Ray property");
	    }
    }
}
