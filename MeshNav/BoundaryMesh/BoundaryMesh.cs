﻿using System.Collections.Generic;
using System.Linq;
using MeshNav.TraitInterfaces;

namespace MeshNav.BoundaryMesh
{
    public class BoundaryMesh : Mesh
    {
        #region Private variables
        private readonly HashSet<HalfEdge> _boundaryEdges = new HashSet<HalfEdge>();
        #endregion

        #region Properties
        public List<Face> BoundaryFaces { get; internal set; }
        // This boundary face is used only while building the mesh when we don't have any idea how
        // many boundaries may be contained in the mesh.  That will be determined in FinalizeMesh
        // when the list of BoundaryFaces above will be constructed.
        public override Face BoundaryFace => null;
        #endregion

        #region Overrides
        protected override void AddBoundaryEdgeHook(HalfEdge edge)
        {
            _boundaryEdges.Add(edge);
        }

        protected override void ChangeBoundaryToInternalHook(HalfEdge edge)
        {
            _boundaryEdges.Remove(edge);
        }

	    public override void SetOrientation(bool fCcw)
	    {
			// We have to deal with boundary faces first.  Outer boundaries have the opposite orientation of all
			// other faces while interior hole boundaries have the same orientation
		    foreach (var face in BoundaryFaces)
		    {
				// If we forget about outer boundaries, then we want to reverse when face.IsCcw != fCcw.  The sense of
				// this is reversed if we do have an outer boundary so the proper bool is (face.IsCcw != fCcw) ^ IsOuterBoundary
			    if ((face.IsCcw != fCcw) ^ ((BoundaryFace)face).IsOuterBoundary)
			    {
				    face.Reverse();
			    }
			}
			base.SetOrientation(fCcw);
	    }
	    #endregion

        #region Constructor
        ////////////////////////////////////////////////////////////////////////////////////////////////////
	    ///  <summary>   Constructor. </summary>
	    /// 
	    ///  <remarks>   Construct a mesh whose points have a given dimension
	    ///              Darrell Plank, 12/7/2017. </remarks>
	    /// 
	    ///  <param name="dimension">    The dimension. </param>
	    ///  <param name="factory">			Factory for the subclass of this mesh </param>
	    ////////////////////////////////////////////////////////////////////////////////////////////////////
	    internal BoundaryMesh(int dimension, Factory factory) : base(dimension, factory)
	    {
            BoundaryFaces = new List<Face>();
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
                FacesInternal.Add(boundaryFace);
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
                    var nextVertex = curEdge.NextVertex;
                    var foundNextEdge = false;
                    foreach (var nextEdge in MapVerticesToEdges[nextVertex])
                    {
                        if (nextEdge.Face == null)
                        {
                            if (foundNextEdge)
                            {
                                // Uh oh - found three outside edges at this vertex - not good
                                throw new MeshNavException("Found three outside edges at a node");
                            }
                            foundNextEdge = true;
                            curEdge.NextEdge = nextEdge;
                            nextEdge.PreviousEdge = curEdge;

                            curEdge = nextEdge;
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
