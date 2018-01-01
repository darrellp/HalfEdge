using System.Collections.Generic;
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

		/// <summary>
		/// Clone the new vertex from the old vertex.
		/// </summary>
		/// 
		/// <remarks> The position has been set.  No new HalfEdges are available yet and should be
		/// set in Mesh.PatchClone(). We have to set the old edge though so we can reference it
		/// in PatchClone. </remarks>
		/// 
		/// <param name="newVertex"> The vertex being cloned into </param>
		/// <param name="oldVertex"> The vertex being cloned from </param>
	    protected internal virtual void CloneVertex(Vertex newVertex, Vertex oldVertex)
		{
			newVertex.Edge = oldVertex.Edge;
		}

		/// <summary>
		/// Clone the new halfedge from the old halfedge.
		/// </summary>
		/// 
		/// <remarks> The only thing set to this point is the initial vertex.  No new halfedges
		/// or faces are available yet and should be set in Mesh.PatchClone.  We have to set the
		/// face. opposite and nextEdge however so they can be references in PatchClone. </remarks>
		/// 
		/// <param name="newHalfEdge"> New halfedge to clone into </param>
		/// <param name="halfEdge"> Old halfedge we're cloning from </param>
		/// <param name="oldToNewVertex"> Mapping from old vertices to new vertices </param>
		protected internal virtual void CloneHalfEdge(HalfEdge newHalfEdge, HalfEdge halfEdge, Dictionary<Vertex, Vertex> oldToNewVertex)
		{
			newHalfEdge.NextEdge = halfEdge.NextEdge;
			newHalfEdge.Face = halfEdge.Face;
		    newHalfEdge.Opposite = halfEdge.Opposite;
		}

		/// <summary>
		/// Clone the new face from the old face.
		/// </summary>
		/// 
		/// <remarks> This has already had it's halfedge set so really nothing to do for normal mesh </remarks>
		/// <param name="newFace"> New face to clone to </param>
		/// <param name="face"> Face we're cloning from </param>
		/// <param name="oldToNewHalfEdge"> Mapping from old HalfEdges to new </param>
		/// <param name="oldToNewVertex"> Mapping from old vertices to new </param>
		protected internal virtual void CloneFace(Face newFace, Face face, Dictionary<HalfEdge, HalfEdge> oldToNewHalfEdge, Dictionary<Vertex, Vertex> oldToNewVertex)
	    {
	    }
    }
}
