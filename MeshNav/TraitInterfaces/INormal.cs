using System;
using MathNet.Numerics.LinearAlgebra;

namespace MeshNav.TraitInterfaces
{
    public interface INormal<T> where T : struct, IEquatable<T>, IFormattable
	{
        Vector<T> NormalAccessor { get; set; }
    }
}
