using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.TraitInterfaces;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
    public class Face : MeshComponent
    {
        #region Public Properties
        public HalfEdge HalfEdge { get; internal set; }
	    public Mesh Mesh => HalfEdge.Mesh;
        #endregion

        #region Traits
        public bool IsBoundary
        {
            // ReSharper disable PossibleNullReferenceException
            get => !Mesh.BoundaryTrait && (this as IBoundary).IsBoundaryAccessor;
            set
            {
                if (Mesh.BoundaryTrait)
                {
                    (this as IBoundary).IsBoundaryAccessor = value;
                }
            }
            // ReSharper restore PossibleNullReferenceException
        }
        #endregion

        #region Constructor
        internal Face() {}
        #endregion

        #region Accessors
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Enumerates edges of this face. </summary>
        ///
        /// <remarks>   Darrell Plank, 1/2/2018. </remarks>
        ///
        /// <returns>
        /// An enumerator of the edges in this face.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IEnumerable<HalfEdge> Edges()
        {
            if (HalfEdge == null)
            {
                // This occurs on the BoundaryFace when there are no border edges
                yield break;
            }
            var curEdge = HalfEdge;
            do
            {
                yield return curEdge;
                curEdge = curEdge.NextEdge;
            } while (curEdge != HalfEdge);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Enumerates the vertices of this face. </summary>
        ///
        /// <remarks>   Darrell Plank, 1/2/2018. </remarks>
        ///
        /// <returns>
        /// An enumerator of the vertices of this face.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IEnumerable<Vertex> Vertices()
        {
            return Edges().Select(e => e.InitVertex);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Enumerates points in this face. </summary>
        ///
        /// <remarks>   "points" here are the Vectors at each vertex as opposed to the Vertex objects
        ///             returned (oddly enough) by Vertices().
        ///             Darrell Plank, 1/2/2018. </remarks>
        ///
        /// <returns>
        /// An enumerator of points in this face.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IEnumerable<Vector<T>> Points()
        {
            return Vertices().Select(v => v.Position);
        }
        #endregion

        #region Validation
        internal bool Validate()
        {
            var edges = new HashSet<HalfEdge>();
            var lastEdge = (HalfEdge)null;
            foreach (var halfEdge in Edges())
            {
                if (edges.Contains(halfEdge))
                {
                    throw new MeshNavException("Edges loop back on themselves");
                }
                if (lastEdge != null && lastEdge.NextEdge != halfEdge)
                {
                    throw new MeshNavException("Improper edge linkage within face");
                }
                lastEdge = halfEdge;
                edges.Add(halfEdge);
            }
            return true;
        }
        #endregion

        #region Calculations
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Signed area of a face.
        /// </summary>
        ///
        /// <remarks>   It's positive if the points are in counterclockwise order,
        ///             negative otherwise and it's absolute value is the area of the polygon.
        ///             Darrell Plank, 1/1/2018. </remarks>
        ///
        /// <returns>   Signed area of a polygon. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
 
        // ReSharper disable once InconsistentNaming
        public virtual int ICcw()
        {
            if (Mesh.Factory.Dimension != 2)
            {
                throw new MeshNavException("Calling ICcw on non 2D mesh");
            }
            return Math.Sign(Geometry2D.SignedArea(Points().ToList()));
        }
        #endregion

        #region Modification
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Reverses the orientation of this face. </summary>
        ///
        /// <remarks>   Doesn't affect any OppositeEdges making the mesh temporarily invalid 
        ///             Darrell Plank, 1/2/2018. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void Reverse()
        {
            var edgeList = Edges().ToList();
            var vtxList = edgeList.Select(e => e.InitVertex).ToList();
            for (var i = 0; i < edgeList.Count; i++)
            {
                var iNext = (i + 1) % edgeList.Count;
                var iPrev = (i + edgeList.Count - 1) % edgeList.Count;
                var edge = edgeList[i];
                edge.NextEdge = edgeList[iPrev];
                edge.InitVertex = vtxList[iNext];
                edge.PreviousEdge = edgeList[iNext];
            }
        }
        #endregion
    }
}