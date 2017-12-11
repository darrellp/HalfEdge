using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshNav.TraitInterfaces
{
    public interface IPreviousEdge<T> where T : struct, IEquatable<T>, IFormattable
    {
        HalfEdge<T> PreviousEdgeAccessor { get; set; }
    }
}
