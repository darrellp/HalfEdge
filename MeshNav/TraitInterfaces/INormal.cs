using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.TraitInterfaces
{
    public interface INormal
	{
        Vector<T> NormalAccessor { get; set; }
    }
}
