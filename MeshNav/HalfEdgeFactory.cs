﻿using System;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using Assimp;

namespace MeshNav
{
    public class HalfEdgeFactory<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Static Variables
        protected static readonly VectorBuilder<T> Builder = Vector<T>.Build;
        protected static readonly VectorBuilder<float> FBuilder = Vector<float>.Build;
        private readonly bool fFloatType;
        #endregion

        #region Private Variables
        #endregion

        #region Public properties
        public int Dimension { get;}
        #endregion

        #region Constructor
        public HalfEdgeFactory(int dimension)
        {
            Dimension = dimension;
            if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            {
                throw new MeshNavException("MeshNav only accepts types of float and double");
            }
            fFloatType = typeof(T) == typeof(float);
        }
        #endregion

        #region Virtual functions
        internal Vector<T> FromVector3D(Vector3D vec)
        {
            if (fFloatType)
            {
                return Vector<float>.Build.DenseOfArray(new[] { vec.X, vec.Y, vec.Z }) as Vector<T>;
            }
            // ReSharper disable once RedundantCast
            return Vector<double>.Build.DenseOfArray(new[] { (double)vec.X, (double)vec.Y, (double)vec.Z }) as Vector<T>;
        }

        public virtual Mesh<T> CreateMesh()
        {
            return new Mesh<T>(Dimension);
        }

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
