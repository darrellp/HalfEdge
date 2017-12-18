using System;
using System.Collections.Generic;
using System.Linq;

namespace MeshNav.BoundaryMesh
{
    class BoundaryMesh<T> : Mesh<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Private variables
        private readonly BoundaryMeshFace<T> _boundaryFace;
        // We don't currently allow for separate components in a boundary mesh.  Otherwise we break the notion that all the adjacent edges
        // to a face can be enumerated by starting at one HE and then going from one to the next.  A separate component would mean the
        // face at infinity had to enumerate through two sets of border edges.  We keep count of the number of outside edges so
        // that when we set them all at the finalization we can count and make sure the two counts match - otherwise we have bad
        // topology.
        //
        // Strictly speaking, we don't allow two "outer edges".  For 3D closed objects there are not any edges but there may still
        // be multiple components - two spheres for instance.  Do we need to check for this situation since there is no "face at
        // infinity" which needs to be consistent?  Not sure.
        private int _boundaryCount;
        #endregion

        #region Properties
//        protected override Face<T> BoundaryFace => _boundaryFace;
        public override Face<T> BoundaryFace => _boundaryFace;
        #endregion

        #region Overrides
        protected override void AddBoundaryEdgeHook(HalfEdge<T> edge)
        {
            _boundaryCount++;
            _boundaryFace.HalfEdge = edge;
        }

        protected override void ChangeBoundaryToInternalHook(HalfEdge<T> halfEdge)
        {
            _boundaryCount--;
        }

        protected override HalfEdgeFactory<T> GetFactory(int dimension)
        {
            return new BoundaryMeshFactory<T>(dimension);
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
	        _boundaryFace = (BoundaryMeshFace < T >)HalfEdgeFactory.CreateFace();
	        _boundaryFace.IsBoundaryAccessor = true;
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
        // TODO: Mull over the stuff in the comment above
        protected override void FinalizeHook()
        {
            if (_boundaryCount == 0)
            {
                _boundaryFace.HalfEdge = null;
                return;
            }
            var edgeCount = 0;
            var curEdge = _boundaryFace.HalfEdge;
            if (curEdge.NextEdge != null)
            {
                // Don't think this can happen since we only 
                throw new MeshNavException("Internal error");
            }
            do
            {
                var nextVertex = curEdge.Opposite.InitVertex;
                var foundNextEdge = false;
                foreach (var halfEdge in MapVerticesToEdges[nextVertex])
                {
                    if (halfEdge.Face == _boundaryFace)
                    {
                        if (foundNextEdge)
                        {
                            // Uh oh - found three outside edges at this vertex - not good
                            throw new MeshNavException("Found three outside edges at a node");
                        }
                        foundNextEdge = true;
                        edgeCount++;
                        curEdge.NextEdge = halfEdge;
                        curEdge = halfEdge;
                    }
                }
                if (!foundNextEdge)
                {
                    throw new MeshNavException("Outside Edge has no successor");
                }
            } while (curEdge != _boundaryFace.HalfEdge);

            if (edgeCount != _boundaryCount)
            {
                throw new MeshNavException("More than one connected set of outside edges");
            }
        }
		#endregion

		#region Validation
	    public override bool Validate()
	    {
	        var boundaries = new HashSet<HalfEdge<T>>(BoundaryFace.Edges());
	        if (HalfEdges.Any(halfEdge => halfEdge.Face == BoundaryFace && !boundaries.Contains(halfEdge)))
	        {
	            throw new MeshNavException("Multiple components not allowed in BoundaryMesh");
	        }
	        return base.Validate();
	    }
	    #endregion
	}
}
