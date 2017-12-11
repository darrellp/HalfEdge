using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MeshNav.TraitInterfaces;

namespace MeshNav
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A mesh. </summary>
    ///
    /// <remarks>   All the elements of a single mesh are contained in this structure.  It must have it's
    ///             dimension set before use.  It is created by adding vertices and then adding faces using
    ///             those vertices.
    ///             
    ///             We support various traits through the idea of Traits.  For each trait there is an interface
    ///             in the Traits namespace which applies to either a mesh, vertex, face or halfEdge.  Implementing
    ///             these interfaces in a subclass of the appropriate type enables that trait.  These subclassed
    ///             types must be returned by a HalfEdgeFactory so it needs to also be subclassed.
    ///             
    ///             For instance, to allow PreviousEdges to be stored in HalfEdges, you need to create a new HalfEdge
    ///             class that implements IPreviousEdge.  You also need to subclass HalfEdgeFactory with a factory
    ///             that returns the new HalfEdges.  Finally, Mesh must be subclassed so that Mesh.GetFactory()
    ///             returns the new factory.
    ///             
    ///             Originally I included an object "Tag" on all elements but this should be superseded by subclassing.
    ///             If you want particular information to be stored on particular elements, subclass those elements
    ///             and include that information in the subclass.  Of course, this could include a tag itself if you
    ///             didn't need to information on all elements and wanted to save memory.
    ///             
    ///             Darrell Plank, 12/7/2017. </remarks>
    ///
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Mesh<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Private variables
        private readonly HalfEdgeFactory<T> _halfEdgeFactory;

        // During the construction of the mesh, we can't guarantee valid topologies which means that a lot of the iterators, etc.
        // that we can eventually rely on won't work during construction.  In particular, we can't use AdjacentEdges for vertices.
        // Still, we need to verify which edges emanate from each vertex so during the construction we use a dictionary to
        // map the vertices to the edges which emanate from them.  When we finalize the mesh we can dereference the following
        // dictionary.
        protected Dictionary<Vertex<T>, List<HalfEdge<T>>> MapVerticesToEdges = new Dictionary<Vertex<T>, List<HalfEdge<T>>>();
        #endregion

        #region Properties
        public object Tag { get; set; }
        public IEnumerable<Vertex<T>> Vertices => VerticesInternal;
        public IEnumerable<Face<T>> Faces => FacesInternal;
        public IEnumerable<HalfEdge<T>> HalfEdges => HalfEdgesInternal;
        public bool IsInitialized { get; internal set; }
        internal HalfEdgeFactory<T> HalfEdgeFactory => _halfEdgeFactory;
        protected virtual Face<T> BoundaryFace => null;
        #endregion

        #region Traits
        // We record what traits are supported in the mesh so we don't have to keep doing slow type checks at
        // run time.
        internal bool AtInfinityTrait;
        internal bool NormalsTrait;
        internal bool PreviousEdgeTrait;
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
        public Mesh(int dimension)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            _halfEdgeFactory = GetFactory(dimension);

            // Determine supported traits by checking what Interfaces are supported by the elements

            // ReSharper disable SuspiciousTypeConversion.Global
            var face = HalfEdgeFactory.CreateFace();
            AtInfinityTrait = face is IAtInfinity;
            var vertex = HalfEdgeFactory.CreateVertex(this, Enumerable.Repeat(default(T), dimension).ToArray());
            NormalsTrait = vertex is INormal;
            var halfEdge = HalfEdgeFactory.CreateHalfEdge(null, null, null, null);
            PreviousEdgeTrait = halfEdge is IPreviousEdge<T>;
             // ReSharper restore SuspiciousTypeConversion.Global
       }

	    protected virtual HalfEdgeFactory<T> GetFactory(int dimension)
	    {
		    return new HalfEdgeFactory<T>(dimension);
	    }
        #endregion

        #region Element collections
        internal List<HalfEdge<T>> HalfEdgesInternal = new List<HalfEdge<T>>();
        internal List<Vertex<T>> VerticesInternal = new List<Vertex<T>>();
        internal List<Face<T>> FacesInternal = new List<Face<T>>();
        #endregion

        #region Build Methods
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds a vertex to the mesh. </summary>
        ///
        /// <remarks>   The constructor for InitVertex is internal so this should be the only way a user
        ///             can create a vertex which ensures that any vertices are part of the mesh (well,
        ///             strictly speaking "a" mesh - not specifically this one, but I'm not checking
        ///             on that - I suppose we could add an _containingMesh to InitVertex but it seems like
        ///             a bit of memory hit that is not necessary).
        ///             Darrell Plank, 12/7/2017. </remarks>
        ///
        /// <param name="coords">   A variable-length parameters list containing coordinates. </param>
        ///
        /// <returns>   The new vertex with the given position </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public Vertex<T> AddVertex(params T[] coords)
        {
            if (IsInitialized)
            {
                throw new MeshNavException("Adding vertex to finalized mesh");
            }
            var newVertex = InternalAddVertex(coords);
            MapVerticesToEdges[newVertex] = new List<HalfEdge<T>>();
            return newVertex;
        }

        internal Vertex<T> InternalAddVertex(params T[] coords)
        {
            var newVertex = _halfEdgeFactory.CreateVertex(this, coords);
            VerticesInternal.Add(newVertex);
            return newVertex;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds a face. </summary>
        ///
        /// <remarks>   The passed in vertices are assumed to have already been added to the mesh
        ///             and so are not added here.  Any edge pairs required are added to the edges list.
        ///             Each vertex has it's halfedge assigned from this face.
        ///             Darrell Plank, 12/7/2017. </remarks>
        ///
        /// <exception cref="MeshNavException"> Thrown if there are less than three points. </exception>
        ///
        /// <param name="vertices"> A variable-length parameters list containing the vertices. </param>
        ///
        /// <returns>   The created Face </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual Face<T> AddFace(params Vertex<T>[] vertices)
        {
            if (IsInitialized)
            {
                throw new MeshNavException("Adding face to finalized mesh");
            }
            if (vertices.Length < 3)
            {
                throw new MeshNavException("Degenerate polygon attempted");
            }
            HalfEdge<T> halfEdgePrev = null;
            var newFace = HalfEdgeFactory.CreateFace();
            for (var i = 0; i < vertices.Length; i++)
            {
                var thisVertex = vertices[i];
                if (thisVertex.Mesh != this)
                {
                    throw new MeshNavException("Using vertices which don't belong to this mesh is disallowed");
                }
                var nextIndex = (i + 1) % vertices.Length;
                var nextVertex = vertices[nextIndex];
                HalfEdge<T> halfEdge = null;

                // See if we can find any previously constructed half edges here.  The only way that can happen is
                // if the opposite face has already been constructed since there are only two faces that can
                // construct this edge and we haven't constructed it from our side yet.  If the opposite side
                // constructed it then it could have been constructed in the proper orientation or it could 
                // have been constructed in the wrong orientation which would be an error.
                foreach (var adjacentEdge in MapVerticesToEdges[thisVertex])
                {
                    if (adjacentEdge.Opposite.InitVertex == nextVertex)
                    {
                        // This HE was already constructed by the opposite face.  It's opposite HE should be
                        // pointing to that face.  Our adjacent HE had better be pointing to the outside or else we've
                        // got three faces trying to share an edge or wrong ordering on a face which is bad topology.
                        if (adjacentEdge.Face != BoundaryFace)
                        {
                            // We've got inconsistent ordering on adjacent faces since our adjacent edge should
                            // not have it's face set yet and default to _faceAtInfinity.
                            throw new MeshNavException("Inconsistent vertex ordering on adjacent faces or three faces sharing an edge");
                        }
                        // We've already constructed this half edge from the adjacent face but
                        // we haven't set it's face or previous edge yet so do that and continue on
                        halfEdge = adjacentEdge;
                        ChangeBoundaryToInternalHook(halfEdge);
                        break;
                    }
                }
                if (halfEdge == null)
                {
                    // This edge pair has never been constructed so do it ourselves
                    
                    // Initially set the opposite face to the face at infinity
                    var opposite = HalfEdgeFactory.CreateHalfEdge(nextVertex, null, BoundaryFace, null);
                    AddBoundaryEdgeHook(opposite);
                    halfEdge = HalfEdgeFactory.CreateHalfEdge(thisVertex, opposite, newFace, null);
                    opposite.Opposite = halfEdge;
                    HalfEdgesInternal.Add(halfEdge);
                    HalfEdgesInternal.Add(opposite);
                    MapVerticesToEdges[thisVertex].Add(halfEdge);
                    MapVerticesToEdges[nextVertex].Add(opposite);
                }

                halfEdge.Face = newFace;

                if (i == 0)
                {
                    newFace.HalfEdge = halfEdge;
                }
                if (halfEdgePrev != null)
                {
                    halfEdgePrev.NextEdge = halfEdge;
                    halfEdge.PreviousEdge = halfEdge;
                }
                halfEdgePrev = halfEdge;
            }
            Debug.Assert(halfEdgePrev != null, "halfEdgePrev != null");
          
            halfEdgePrev.NextEdge = newFace.HalfEdge;
            FacesInternal.Add(newFace);
            return newFace;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Finalizes the mesh. </summary>
        ///
        /// <remarks>   Validates the mesh and sets up the outside edges.
        ///             Darrell Plank, 12/7/2017. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void FinalizeMesh()
        {
            if (IsInitialized)
            {
                return;
            }

            FinalizeHook();

            // Dereference map created during construction
            MapVerticesToEdges = null;

            if (!Validate())
            {
                throw new MeshNavException("Invalid created mesh");
            }
            IsInitialized = true;
        }
        #endregion

        #region Validation
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Validates this mesh. </summary>
        ///
        /// <remarks>   TODO: Think about this
        ///             How much do we really want to do at this top level?  Without making certain assumptions
        ///             it's really difficult to do anything useful but we want to be as flexible as possible.
        ///             
        ///             Darrell Plank, 12/8/2017. </remarks>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual bool Validate()
        {
            if (VerticesInternal.Count == 0)
            {
                if (HalfEdgesInternal.Count != 0 && FacesInternal.Count != 0)
                {
                    throw new MeshNavException("No vertices but we have other elements");
                }
            }

            if (Faces.Any(face => face.Edges().Take(3).Count() != 3))
            {
                throw new MeshNavException("Faces with less than three sides not allowed");
            }
            return true;
        }
		#endregion

		#region Hooks
	    protected virtual void AddBoundaryEdgeHook(HalfEdge<T> opposite) { }
	    protected virtual void FinalizeHook() { }
        protected virtual void ChangeBoundaryToInternalHook(HalfEdge<T> halfEdge) { }
		#endregion
	}
}
