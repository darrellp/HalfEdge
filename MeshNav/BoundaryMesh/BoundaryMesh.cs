using System;
using System.Collections.Generic;
using System.Linq;
using MeshNav.TraitInterfaces;

namespace MeshNav.BoundaryMesh
{
    class BoundaryMesh<T> : Mesh<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Private variables
        private readonly HashSet<HalfEdge<T>> _boundaryEdges = new HashSet<HalfEdge<T>>();
        #endregion

        #region Properties
        public List<Face<T>> BoundaryFaces { get; }
        public override Face<T> BoundaryFace => null;
        #endregion

        #region Overrides
        protected override void AddBoundaryEdgeHook(HalfEdge<T> edge)
        {
            _boundaryEdges.Add(edge);
        }

        protected override void ChangeBoundaryToInternalHook(HalfEdge<T> edge)
        {
            _boundaryEdges.Remove(edge);
        }

        protected override Factory<T> GetFactory(int dimension)
        {
            return new BoundaryFactory<T>(dimension);
        }

        #endregion

        #region Constructor
        ////////////////////////////////////////////////////////////////////////////////////////////////////
	    /// <summary>   Constructor. </summary>
	    ///
	    /// <remarks>   Construct a mesh whose points have a given dimension
	    ///             Darrell Plank, 12/7/2017. </remarks>
	    ///
	    /// <param name="dimension">    The dimension. </param>
	    ////////////////////////////////////////////////////////////////////////////////////////////////////
	    public BoundaryMesh(int dimension) : base(dimension)
	    {
            BoundaryFaces = new List<Face<T>>();
	    }
        #endregion

        #region Finalization
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Finalizes the mesh. </summary>
        ///
        /// <remarks>   Sets up the outside edges.  Is it possible we could end up
        ///             in an infinite loop that never returns to the original _boundaryFace.HalfEdge?
        ///             Needs mulling over. Only way that could happen is two incoming boundaries meet at a
        ///             single vertex.  We're checking if more that one outside edge exit from a node but
        ///             not two incoming outside edges.  I think that this may be impossible.  Needs more
        ///             mulling over.
        ///             Darrell Plank, 12/7/2017. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void FinalizeHook()
        {
            while (_boundaryEdges.Count > 0)
            {
                var curEdge = _boundaryEdges.First();
                var boundaryFace = Factory.CreateFace();
                BoundaryFaces.Add(boundaryFace);
                // ReSharper disable once PossibleNullReferenceException
                (boundaryFace as IBoundary).IsBoundaryAccessor = true;
                boundaryFace.HalfEdge = curEdge;
                if (curEdge.NextEdge != null)
                {
                    // Don't think this can happen
                    throw new MeshNavException("Internal error");
                }
                do
                {
                    _boundaryEdges.Remove(curEdge);
                    var nextVertex = curEdge.Opposite.InitVertex;
                    var foundNextEdge = false;
                    foreach (var halfEdge in MapVerticesToEdges[nextVertex])
                    {
                        if (halfEdge.Face == null)
                        {
                            if (foundNextEdge)
                            {
                                // Uh oh - found three outside edges at this vertex - not good
                                throw new MeshNavException("Found three outside edges at a node");
                            }
                            foundNextEdge = true;
                            curEdge.NextEdge = halfEdge;
                            curEdge = halfEdge;
                            curEdge.Face = boundaryFace;
                        }
                    }
                    if (!foundNextEdge)
                    {
                        throw new MeshNavException("Outside Edge has no successor");
                    }
                } while (curEdge != boundaryFace.HalfEdge);
            }
        }
		#endregion
	}
}
