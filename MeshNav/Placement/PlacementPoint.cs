using static MeshNav.Utilities;
using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	public class PlacementPoint
	{
		public T X { get; set; }
		public T Y { get; set; }

		public PlacementPoint(Vertex vtx)
		{
			X = vtx.X;
			Y = vtx.Y;
		}

		public PlacementPoint(Vector<T> vec)
		{
			X = vec.X();
			Y = vec.Y();
		}

		public Vector<T> ToVector()
		{
			return Make(X, Y);
		}
	}
}
