using MathNet.Numerics.LinearAlgebra;

namespace MeshNav.TraitInterfaces
{
    public interface INormal
    {
        Vector<double> NormalAccessor { get; set; }
    }
}
