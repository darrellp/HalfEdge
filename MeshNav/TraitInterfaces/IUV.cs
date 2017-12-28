using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.TraitInterfaces
{
	// ReSharper disable once InconsistentNaming
    public interface IUV
    {
        Vector<T> UvAccessor { get; set; }
    }
}
