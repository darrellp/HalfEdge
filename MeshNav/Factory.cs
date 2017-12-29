using MathNet.Numerics.LinearAlgebra;
using Assimp;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
    public class Factory
    {
        #region Static Variables
        protected internal static readonly VectorBuilder<T> Builder = Vector<T>.Build;
        #endregion

        #region Private Variables
        #endregion

        #region Public properties
        public int Dimension { get;}
        #endregion

        #region Constructor
        public Factory(int dimension)
        {
            Dimension = dimension;
        }
        #endregion

        #region Virtual functions
        internal Vector<T> FromVector3D(Vector3D vec)
        {
            // ReSharper disable RedundantCast
            return Builder.DenseOfArray(new[] { (T)vec.X, (T)vec.Y, (T)vec.Z });
            // ReSharper restore RedundantCast
        }

        public virtual Mesh CreateMesh()
        {
            return new Mesh(Dimension, this);
        }

        public Vertex CreateVertex(Mesh mesh, params T[] coords)
        {
            var vector = Builder.Dense(coords);
            return CreateVertex(mesh, vector);
        }

        public virtual Vertex CreateVertex(Mesh mesh, Vector<T> vec)
        {
            if (vec.Count != Dimension)
            {
                throw new MeshNavException("Dimension mismatch");
            }
            return new Vertex(mesh, vec);
        }

        public virtual Face CreateFace()
	    {
		    return new Face();
	    }

	    public virtual HalfEdge CreateHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
	    {
		    return new HalfEdge(vertex, opposite, face, nextEdge);
	    }

		public static Vector<T> Vector(params T[] coords)
        {
            return Builder.Dense(coords);
        }

	    public Vector<T> ZeroVector()
	    {
			return Builder.Dense(Dimension);
	    }
		#endregion
	}
}
