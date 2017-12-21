using System;
using MathNet.Numerics.LinearAlgebra;

namespace MeshNav
{
    public class Factory<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Static Variables
        protected static readonly VectorBuilder<T> Builder = Vector<T>.Build;
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
        public Vertex<T> CreateVertex(Mesh<T> mesh, params T[] coords)
        {
            var vector = Builder.Dense(coords);
            return CreateVertex(mesh, vector);
        }

        public virtual Vertex<T> CreateVertex(Mesh<T> mesh, Vector<T> vec)
        {
            if (vec.Count != Dimension)
            {
                throw new MeshNavException("Dimension mismatch");
            }
            return new Vertex<T>(vec, mesh);
        }


        public virtual Face<T> CreateFace()
	    {
		    return new Face<T>();
	    }

	    public virtual HalfEdge<T> CreateHalfEdge(Vertex<T> vertex, HalfEdge<T> opposite, Face<T> face, HalfEdge<T> nextEdge)
	    {
		    return new HalfEdge<T>(vertex, opposite, face, nextEdge);
	    }

		public static Vector<T> Vector(params T[] coords)
        {
            return Builder.Dense(coords);
        }

        public Vector<double> ZeroVector()
        {
            return Utilities.DblBuilder.Dense(Dimension);
        }
        #endregion
    }
}
