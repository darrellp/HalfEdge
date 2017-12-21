using MathNet.Numerics.LinearAlgebra;

namespace MeshNav.TraitInterfaces
{
	// ReSharper disable once InconsistentNaming
    interface IUV
    {
        Vector<double> UvAccessor { get; set; }
    }
}
