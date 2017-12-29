using MathNet.Numerics.LinearAlgebra;
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
    }
}
