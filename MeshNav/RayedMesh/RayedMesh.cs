using System.Collections.Generic;
using System.Linq;
using MeshNav.TraitInterfaces;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.RayedMesh
{
    public class RayedMesh : Mesh
    {
        #region Private variables
        private int _boundaryCount;
        #endregion

        #region Properties
        public override Face BoundaryFace { get; }
        #endregion

        #region Constructor
        public RayedMesh(int dimension, Factory factory) : base(dimension, factory)
        {
            BoundaryFace = Factory.CreateFace();
            // ReSharper disable once VirtualMemberCallInConstructor
            FacesInternal.Add(BoundaryFace);
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once VirtualMemberCallInConstructor
            (BoundaryFace as IBoundary).IsBoundaryAccessor = true;
        }
        #endregion

        #region Building
        public Vertex AddRayedVertex(params T[] coords)
        {
            if (IsInitialized)
            {
                throw new MeshNavException("Adding vertex to finalized mesh");
            }
            var newVertex = AddVertex(coords);
            // ReSharper disable once PossibleNullReferenceException
            (newVertex as IRayed).IsRayed = true;
            MapVerticesToEdges[newVertex] = new List<HalfEdge>();
            return newVertex;
        }

        internal RayedVertex InternalAddRayedVertex(params T[] coords)
        {
            var newVertex = Factory.CreateVertex(this, coords);
            VerticesInternal.Add(newVertex);
            return newVertex as RayedVertex;
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
                if (VerticesInternal.Count != 0)
                {
                    throw new MeshNavException("Rayed Mesh with vertices has no border edges");
                }
                BoundaryFace.HalfEdge = null;
                return;
            }
            var edgeCount = 0;
            var curEdge = BoundaryFace.HalfEdge;
            if (curEdge.NextEdge != null)
            {
                // Don't think this can happen
                throw new MeshNavException("Internal error");
            }
            do
            {
                var nextVertex = curEdge.NextVertex;
                // ReSharper disable once PossibleNullReferenceException
                if (!(nextVertex as IRayed).IsRayed)
                {
                    throw new MeshNavException("Rayed Mesh has non-rayed vertex on border");
                }
                var foundNextEdge = false;
                foreach (var nextEdge in MapVerticesToEdges[nextVertex])
                {
                    if (nextEdge.Face == BoundaryFace)
                    {
                        if (foundNextEdge)
                        {
                            // Uh oh - found three outside edges at this vertex - not good
                            throw new MeshNavException("Found three outside edges at a node");
                        }
                        foundNextEdge = true;
                        edgeCount++;
                        curEdge.NextEdge = nextEdge;
                        nextEdge.PreviousEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
                if (!foundNextEdge)
                {
                    throw new MeshNavException("Outside Edge has no successor");
                }
            } while (curEdge != BoundaryFace.HalfEdge);

            if (edgeCount != _boundaryCount)
            {
                throw new MeshNavException("More than one connected set of outside edges");
            }
        }
        #endregion

        #region Overrides
        protected override void ValidatePolygon(Vertex[] vertices)
        {
            base.ValidatePolygon(vertices);
            var rayed = Enumerable.Range(0, vertices.Length).Where(i => ((IRayed)vertices[i]).IsRayed).ToArray();
            if (rayed.Length > 0)
            {
                if (rayed.Length != 2)
                {
                    throw new MeshNavException("Number of rayed vertices in a face must be zero or two");
                }
                if (rayed[1] != rayed[0] + 1)
                {
                    if (rayed[1] != vertices.Length - 1 || rayed[0] != 0)
                    {
                        throw new MeshNavException("Rayed vertices must be adjacent");
                    }
                }
            }
        }

        protected override void ChangeBoundaryToInternalHook(HalfEdge halfEdge)
        {
            // ReSharper disable PossibleNullReferenceException
            if (halfEdge.IsAtInfinity)
            {
                throw new MeshNavException("Edge at infinity can only be created once");
            }
        }

        protected override void AddBoundaryEdgeHook(HalfEdge opposite)
        {
            if (opposite.IsAtInfinity)
            {
                _boundaryCount++;
                BoundaryFace.HalfEdge = opposite;
            }
        }
        #endregion
    }
}
