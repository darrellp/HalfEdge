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
        Vector? NormalAccessor { get; set; }
    }
}
