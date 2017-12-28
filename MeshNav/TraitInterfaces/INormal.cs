using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.TraitInterfaces
{
    // Interface for vertices if normals are stored
    public interface INormal
	{
        Vector<T> NormalAccessor { get; set; }
    }
}
