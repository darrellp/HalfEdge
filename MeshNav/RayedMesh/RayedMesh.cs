using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshNav.RayedMesh
{
    public class RayedMesh<T> : Mesh<T> where T : struct, IEquatable<T>, IFormattable
    {
        public RayedMesh(int dimension) : base(dimension) { }

        protected override HalfEdgeFactory<T> GetFactory(int dimension)
        {
            return new RayedHalfEdgeFactory<T>(dimension);
        }
    }
}
