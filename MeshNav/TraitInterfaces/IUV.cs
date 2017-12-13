using MathNet.Numerics.LinearAlgebra;

namespace MeshNav.TraitInterfaces
{
    interface IUV
    {
        Vector<double> UvAccessor { get; set; }
    }
}
