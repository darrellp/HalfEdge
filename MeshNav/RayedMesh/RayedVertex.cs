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
        internal RayedVertex(Mesh mesh, Vector<T> vec) : base(mesh, vec) { }
        public bool IsRayed { get; set; }
    }
}
