using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MeshNav.TraitInterfaces;
using static System.Diagnostics.Debug;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
// ReSharper disable BuiltInTypeReferenceStyle
#endif

namespace MeshNav
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A mesh. </summary>
    ///
    /// <remarks>   All the elements of a single mesh are contained in this structure.  It is created by adding
    ///             vertices and then adding faces using those vertices.
    ///             
    ///             We support various traits through the idea of Traits.  For each trait there is an interface
    ///             in the Traits namespace which applies to either a mesh, vertex, face or halfEdge.  Implementing
    ///             these interfaces in a subclass of the appropriate type enables that trait.  These subclassed
    ///             types must be returned by a Factory so it also needs to also be subclassed.
    ///             
    ///             For instance, to allow PreviousEdges to be stored in HalfEdges, you need to create a new HalfEdge
    ///             class that implements IPreviousEdge.  You also need to subclass Factory with a factory
    ///             that returns the new HalfEdges.
    /// 
    ///				The intention here is to be "factory" centric.  Create everything through properly subclassed factories.
    ///				In order to make this easier there is a "template.cs" file which can be copied into a project and with
    ///				a bit of defining the desired traits and one search and replace, you can usually create the mesh with
    ///				all the desired traits you'd like.  If you want something beyond the supported traits you may have to
    ///				do a little extra work.  There are simple instructions in the comments at the top of template.cs.
    /// 
    ///				TODO: We should probably make Mesh and a distinct ImmutableMesh for initialized meshes
    ///				That way we could partially construct a Mesh and produce the finalized version and then add more
    ///				faces and produce another mesh.  Also, it would distinguish more readily initialized meshes and
    ///				uninitialized ones and emphasize that the former are immutable.
    ///             
    ///             Darrell Plank, 12/7/2017. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Mesh
    {
        #region Private variables
        // During the construction of the mesh, we can't guarantee valid topologies which means that a lot of the iterators, etc.
        // that we can eventually rely on won't work during construction.  In particular, we can't use AdjacentEdges for vertices.
        // Still, we need to verify which edges emanate from each vertex so during the construction we use a dictionary to
        // map the vertices to the edges which emanate from them.  When we finalize the mesh we can dereference the following
        // dictionary.
        protected Dictionary<Vertex, List<HalfEdge>> MapVerticesToEdges = new Dictionary<Vertex, List<HalfEdge>>();
        #endregion

        #region Properties
        public object Tag { get; set; }
		// TODO: Should we cache these if IsInitialized?  We need to return ReadOnlyCollection because

		// We need to wrap these in ReadOnlyCollection or else the caller could feasibly cast the
		// IEnumerable as a list and then alter its contents.
        public IEnumerable<Vertex> Vertices => new ReadOnlyCollection<Vertex>(VerticesInternal);
        public IEnumerable<Face> Faces => new ReadOnlyCollection<Face>(FacesInternal);
        public IEnumerable<HalfEdge> HalfEdges => new ReadOnlyCollection<HalfEdge>(HalfEdgesInternal);
        public bool IsInitialized { get; private set; }

        internal Factory Factory { get; }
        public virtual Face BoundaryFace => null;
	    public int Dimension => Factory.Dimension;
        #endregion

        #region Traits
        // We record what traits are supported in the mesh so we don't have to keep doing slow type checks at
        // run time.
        internal bool BoundaryTrait;
        internal bool NormalsTrait;
        internal bool PreviousEdgeTrait;
        internal bool RayedTrait;
        internal bool UvTrait;
        #endregion
 
        #region Element collections
        internal List<HalfEdge> HalfEdgesInternal = new List<HalfEdge>();
        internal List<Vertex> VerticesInternal = new List<Vertex>();
        internal List<Face> FacesInternal = new List<Face>();

		// Returns only one representative of every halfedge pair
	    public IEnumerable<HalfEdge> Edges()
	    {
			var alreadyInserted = new HashSet<HalfEdge>();
		    foreach (var halfEdge in HalfEdgesInternal)
		    {
			    if (alreadyInserted.Contains(halfEdge))
			    {
				    continue;
			    }

			    yield return halfEdge;
			    alreadyInserted.Add(halfEdge.Opposite);
		    }
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
	    ///  <param name="factory">		The factory for this mesh subclass </param>
	    ////////////////////////////////////////////////////////////////////////////////////////////////////
	    internal Mesh(int dimension, Factory factory)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Factory = factory;

            // Determine supported traits by checking what Interfaces are supported by the elements

            // ReSharper disable SuspiciousTypeConversion.Global
            var face = Factory.CreateFace();
            var vertex = Factory.CreateVertex(this, Enumerable.Repeat(default(T), dimension).ToArray());
            var halfEdge = Factory.CreateHalfEdge(null, null, null, null);

            BoundaryTrait = face is IBoundary;
            NormalsTrait = vertex is INormal;
            UvTrait = vertex is IUV;
            PreviousEdgeTrait = halfEdge is IPreviousEdge;
	        RayedTrait = vertex is IRayed;
		}
		#endregion

		#region Queries
	    public (T Top, T Bottom, T Left, T Right) VertexBounds(T expansion = 0)
	    {
			var top = T.MinValue;
			var bottom = T.MinValue;
			var right = T.MinValue;
			var left = T.MaxValue;

			foreach (var vertex in Vertices)
			{
				var x = vertex.X;
				var y = vertex.Y;
				if (x > right)
				{
					right = x;
				}

				if (x < left)
				{
					left = x;
				}

				if (y > top)
				{
					top = y;
				}

				if (y < bottom)
				{
					bottom = y;
				}
			}

			return (top + expansion, bottom - expansion, left - expansion, right + expansion);
	    }
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
		public Vertex AddVertex(params T[] coords)
        {
            if (IsInitialized)
            {
                throw new MeshNavException("Adding vertex to finalized mesh");
            }
            return AddVertex(Factory.CreateVertex(this, coords));
        }

        public Vertex AddVertex(Vector vec)
        {
            if (IsInitialized)
            {
                throw new MeshNavException("Adding vertex to finalized mesh");
            }
            return AddVertex(Factory.CreateVertex(this, vec));
        }

        internal Vertex AddVertex(Vertex vtx)
        {
            if (IsInitialized)
            {
                throw new MeshNavException("Adding vertex to finalized mesh");
            }
            VerticesInternal.Add(vtx);
            MapVerticesToEdges[vtx] = new List<HalfEdge>();
            return vtx;
        }

        public Face AddFace(IEnumerable<int> indices)
	    {
		    return AddFaceEnumerable(indices.Select(i =>
		    {
		        if (i < 0 || i >= VerticesInternal.Count)
		        {
		            throw new MeshNavException("Index out of bounds in AddFace");
		        }
		        return VerticesInternal[i];
		    }));
	    }

	    public Face AddFace(params Vertex[] vertices)
	    {
			return AddFaceEnumerable(vertices);

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
        /// <param name="verticesEn"> An enumerable containing the vertices. </param>
        ///
        /// <returns>   The created Face </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual Face AddFaceEnumerable(IEnumerable<Vertex> verticesEn)
        {
            var vertices = verticesEn.ToArray();
            ValidatePolygon(vertices);
            HalfEdge halfEdgePrev = null;
            var newFace = Factory.CreateFace();
            for (var i = 0; i < vertices.Length; i++)
            {
                var thisVertex = vertices[i];
                
                if (thisVertex.Mesh != this)
                {
                    throw new MeshNavException("Using vertices which don't belong to this mesh is disallowed");
                }
                var nextIndex = (i + 1) % vertices.Length;
                var nextVertex = vertices[nextIndex];
                HalfEdge halfEdge = null;

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
                    var opposite = Factory.CreateHalfEdge(nextVertex, null, BoundaryFace, null);
                    halfEdge = Factory.CreateHalfEdge(thisVertex, opposite, newFace, null);
                    opposite.Opposite = halfEdge;
                    AddBoundaryEdgeHook(opposite);
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
                    halfEdge.PreviousEdge = halfEdgePrev;
                }
                halfEdgePrev = halfEdge;
            }
            Assert(halfEdgePrev != null, "halfEdgePrev != null");
          
            halfEdgePrev.NextEdge = newFace.HalfEdge;
            newFace.HalfEdge.PreviousEdge = halfEdgePrev;
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Makes a deep copy of this object. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/31/2017. </remarks>
        ///
        /// <returns>   A copy of this object. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
	    public Mesh Clone()
	    {
		    var newMesh = Factory.CreateMesh();

			var oldToNewVertex = new Dictionary<Vertex, Vertex>();
			var oldToNewHalfEdge = new Dictionary<HalfEdge, HalfEdge>();
			var oldToNewFace = new Dictionary<Face, Face>();
		    var newVertices = new List<Vertex>();
		    var newHalfEdges = new List<HalfEdge>();
		    var newFaces = new List<Face>();

		    foreach (var vertex in Vertices.Where(v => v.IsAlive))
		    {
			    var newVertex = Factory.CreateVertex(newMesh, vertex.Position.Clone());
			    Factory.CloneVertex(newVertex, vertex);
			    newVertices.Add(newVertex);
			    oldToNewVertex[vertex] = newVertex;
		    }

		    foreach (var halfEdge in HalfEdges.Where(he => he.IsAlive))
		    {
			    var newHalfEdge = Factory.CreateHalfEdge(oldToNewVertex[halfEdge.InitVertex],
					null, null, null);
				Factory.CloneHalfEdge(newHalfEdge, halfEdge, oldToNewVertex);
			    newHalfEdges.Add(newHalfEdge);
			    oldToNewHalfEdge[halfEdge] = newHalfEdge;
		    }

		    foreach (var face in Faces.Where(f => f.IsAlive))
		    {
			    var newFace = Factory.CreateFace();
			    newFace.HalfEdge = oldToNewHalfEdge[face.HalfEdge];
				Factory.CloneFace(newFace, face, oldToNewHalfEdge, oldToNewVertex);
			    newFaces.Add(newFace);
			    oldToNewFace[face] = newFace;
		    }

		    newMesh.VerticesInternal = newVertices;
		    newMesh.HalfEdgesInternal = newHalfEdges;
		    newMesh.FacesInternal = newFaces;

		    newMesh.PatchClone(
                this,
			    oldToNewVertex,
			    oldToNewHalfEdge,
			    oldToNewFace);
#if DEBUG
	        newMesh.Validate();
#endif
            return newMesh;
	    }

        /// <summary>
        /// Final patching of components in the mesh
        /// </summary>
        /// 
        /// <remarks> We don't have all the information necessary to finalize the components when
        /// we first clone so this gives us a final pass to patch everything up. The order of cloning
        /// is vertices, then halfedges and finally faces.  This means that vertices don't have any
        /// access to any other components, halfedges don't have access to other halfedges or faces
        /// and faces don't have any access to other faces.  Any of these components need to be patched
        /// up here. </remarks>
        /// <param name="oldMesh"> Previous mesh we're cloning from </param>
        /// <param name="oldToNewVertex"> Mapping </param>
        /// <param name="oldToNewHalfEdge"> Mapping </param>
        /// <param name="oldToNewFace"> Mapping </param>
        protected virtual void PatchClone(Mesh oldMesh, Dictionary<Vertex, Vertex> oldToNewVertex, Dictionary<HalfEdge, HalfEdge> oldToNewHalfEdge, Dictionary<Face, Face> oldToNewFace)
	    {
            // Vertex edges got set up in Edge constructor so no need to patch that here

		    foreach (var halfEdge in HalfEdges)
		    {
                // These components were unavailable during the first pass through cloning and we stored the
                // old components in their place - time to fix that.
			    halfEdge.Face = oldToNewFace[halfEdge.Face];
			    halfEdge.NextEdge = oldToNewHalfEdge[halfEdge.NextEdge];
		        halfEdge.Opposite = oldToNewHalfEdge[halfEdge.Opposite];
		    }
	    }

        #endregion

        #region Modification
        public virtual void SetOrientation(bool fCcw)
        {
            if (Factory.Dimension != 2)
            {
                throw new MeshNavException("Can't perform SetOrientation on non-planar mesh");
            }
			
            foreach (var face in Faces)
            {
                if (face.IsBoundary)
                {
                    // We have to take care of boundary faces individually.
                    continue;
                }
                // It would be nice if we could eliminate the call to ICcw here and if
                // it weren't for the possibility of different components we could.  Even in
                // the presence of components, we could do it if we had the ability to identify
                // each component and step through all it's faces since all faces of individual
                // components will be oriented the same way.  Something to think about for the
                // future, but right now we don't have this facility.  In the case of rayed
                // meshes we could since we're always guaranteed exactly one component.  In the
                // case of Boundary meshes it might be possible with a bit more analysis since
                // we have one boundary per components except for the presence of holes.
                // In the case of a null boundary, this would be difficult to impossible.  I may
                // have to revisit and make this virtual so the different cases handle it their own
                // way.
                if (fCcw ^ face.IsCcw)
                {
	                face.Reverse();
                }
			}
#if DEBUG
	        Validate();
#endif
        }
        #endregion

        #region Validation
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Validates this mesh. </summary>
        ///
        /// <remarks>   TODO: Think about this
        ///             How much do we really want to do at this top level?  Without making certain assumptions
        ///             it's really difficult to do anything useful but we want to be as flexible as possible.
        ///             Since we currently only create elements by adding faces, it's pretty safe, I think, to
        ///             insist that all faces have at least three edges.
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

            if (Faces.Any(face => face.Edges().Take(3).Count() != 3 && !face.IsBoundary))
            {
                throw new MeshNavException("Faces with less than three sides not allowed");
            }

            if (Faces.Where(f => f.IsAlive).Any(f => !f.HalfEdge.IsAlive))
            {
                throw new MeshNavException("Live faces point to dead halfedges");
            }

            if (Vertices.Where(v => v.IsAlive).Any(v => v.Mesh != this || !v.Edge.IsAlive))
            {
                throw new MeshNavException("Live vertices point at dead half edges or don't belong to this mesh");
            }

            if (HalfEdges.Where(he => he.IsAlive).Any(he => !he.InitVertex.IsAlive || he.Face != null && !he.Face.IsAlive))
            {
                throw new MeshNavException("Live halfedge points at dead component");
            }

	        if (HalfEdges.Where(he => he.IsAlive).Any(he => he.InitVertex == he.Opposite.InitVertex))
	        {
		        throw new MeshNavException("Edge and opposite oriented identically");
	        }
            return true;
        }
        #endregion

		#region Hooks
	    protected virtual void AddBoundaryEdgeHook(HalfEdge opposite) { }
	    protected virtual void FinalizeHook() { }
        protected virtual void ChangeBoundaryToInternalHook(HalfEdge halfEdge) { }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Validates the polygon described by vertices. </summary>
        ///
        /// <remarks>   Darrell Plank, 1/3/2018. </remarks>
        ///
        /// <exception cref="MeshNavException"> Thrown when a Mesh Navigation error condition occurs. </exception>
        ///
        /// <param name="vertices"> The vertices. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void ValidatePolygon(Vertex[] vertices)
        {
            // TODO: Check that the polygon is simple
            // http://geomalgorithms.com/a09-_intersect-3.html#simple_Polygon
            if (IsInitialized)
            {
                throw new MeshNavException("Adding face to finalized mesh");
            }
            if (vertices.Length < 3)
            {
                throw new MeshNavException("Degenerate polygon attempted");
            }
        }
        #endregion
    }
}
