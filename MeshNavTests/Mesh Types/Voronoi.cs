﻿#pragma warning disable 1587
////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A template factory. </summary>
///
/// <remarks>   To Use:
///             1. Copy to your own Build.  
///             2. Rename the file if desired   
///             3. Uncomment the TEMPLATE define below  
///             4. Define exactly one boundary type  
///             5. Define whatever other traits are desired  
///             6. Rename the namespace if desired  
///             7. Search and replace "Voronoi" with an arbitrary prefix for your project  
///             
///             After this, you should be able to build.  If you picked MyMesh for your prefix
///             then your classes will be MyMeshFactory, MyMeshMesh, etc.
///             Darrell Plank, 12/28/2017. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma warning restore 1587

#define TEMPLATE
#if TEMPLATE

// Define only ONE of the following three defines to specify how boundary conditions are handled

//#define NULLBOUNDARY
//#define BOUNDARY
#define RAYED

// Pick any of the following
//#define NORMALS
#define PREVIOUSEDGE
//#define UV
#if RAYED
// VORONOI is subclass of RAYED
#define VORONOI
#endif

#region Template Definition
#if (RAYED && BOUNDARY) || (RAYED && NULLBOUNDARY) || (BOUNDAY && NULLBOUNDARY) || (!BOUNDARY && !NULLBOUNDARY && !RAYED)
#error Precisely one boundary condition should be defined in the template
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

// ReSharper disable RedundantUsingDirective
using MeshNav;
using MeshNav.RayedMeshSpace;
using MeshNav.BoundaryMesh;
using MeshNav.TraitInterfaces;
// ReSharper restore RedundantUsingDirective
// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming

////////////////////////////////////////////////////////////////////////////////////////////////////
// namespace: MeshNav.TemplateFactory
//
// summary:	Rename this if desired
// 
////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Templates
{
    public partial class VoronoiFactory : Factory
    {
        public VoronoiFactory(int dimension) : base(dimension) { }

        public override Mesh CreateMesh()
        {
            return new VoronoiMesh(Dimension, this);
        }

        public override Face CreateFace()
        {
            return new VoronoiFace();
        }

        public override HalfEdge CreateHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
        {
            return new VoronoiHalfEdge(vertex, opposite, face, nextEdge);
        }

        internal override Vertex CreateVertex(Mesh mesh, Vector vec)
        {
            return new VoronoiVertex(mesh, vec);
        }

        protected internal override void CloneVertex(Vertex newVertex, Vertex oldVertex)
        {
            base.CloneVertex(newVertex, oldVertex);
            FactoryNormalsCloneVertex(newVertex, oldVertex);
            FactoryUVCloneVertex(newVertex, oldVertex);
        }

        protected internal override void CloneHalfEdge(HalfEdge newHalfEdge, HalfEdge halfEdge, Dictionary<Vertex, Vertex> oldToNewVertex)
        {
            base.CloneHalfEdge(newHalfEdge, halfEdge, oldToNewVertex);
            FactoryPreviousEdgeCloneHalfedge(newHalfEdge, halfEdge);
        }
    }

    public partial class VoronoiMesh :
#if BOUNDARY
        BoundaryMesh
#elif RAYED
        RayedMesh
#else
        Mesh
#endif
    {
        public VoronoiMesh(int dimension, Factory factory) : base(dimension, factory) { }

        protected override void PatchClone(Mesh oldMesh, Dictionary<Vertex, Vertex> oldToNewVertex, Dictionary<HalfEdge, HalfEdge> oldToNewHalfEdge, Dictionary<Face, Face> oldToNewFace)
        {
            base.PatchClone(oldMesh, oldToNewVertex, oldToNewHalfEdge, oldToNewFace);
            MeshBoundaryHalfClone(oldMesh, oldToNewFace);
            MeshPreviousEdgeHalfClone(oldToNewHalfEdge);
        }

        public override bool Validate()
        {
            base.Validate();
            MeshPreviousEdgeValidate();
            return true;
        }
    }

	public class VoronoiFace :
#if BOUNDARY
		BoundaryFace
#elif RAYED || VORONOI
        RayedFace
#else
        Face
#endif
#if VORONOI
	, IVoronoi
#endif
	{
#if VORONOI
		public Vector VoronoiPoint { get; set; }
#endif
	}

    public class VoronoiHalfEdge : HalfEdge
#if PREVIOUSEDGE
        , IPreviousEdge
#endif
    {
        public VoronoiHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
            : base(vertex, opposite, face, nextEdge)
        {
        }

#if PREVIOUSEDGE
        public HalfEdge PreviousEdgeAccessor { get; set; }
#endif
    }

    public class VoronoiVertex :
#if RAYED
        RayedVertex
#else
        Vertex
#endif

#if NORMALS
        , INormal
#endif
#if RAYED
        ,IRayed
#endif
#if UV
        , IUV
#endif
    {
        internal VoronoiVertex(Mesh mesh, Vector vec) : base(mesh, vec) { }

#if NORMALS
        public Vector? NormalAccessor { get; set; }
#endif

#if UV
        public Vector? UvAccessor { get; set; }
#endif
    }

#region Conditionals
	public partial class VoronoiFactory
	{
		[Conditional("NORMALS")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void FactoryNormalsCloneVertex(Vertex newVertex, Vertex oldVertex)
		{
			(newVertex as INormal).NormalAccessor = (oldVertex as INormal).NormalAccessor;
		}

		[Conditional("UV")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void FactoryUVCloneVertex(Vertex newVertex, Vertex oldVertex)
		{
			(newVertex as IUV).UvAccessor = (oldVertex as IUV).UvAccessor;
		}

		[Conditional("UV")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void FactoryPreviousEdgeCloneHalfedge(HalfEdge newHalfEdge, HalfEdge halfEdge)
		{
			(newHalfEdge as IPreviousEdge).PreviousEdgeAccessor = (halfEdge as IPreviousEdge).PreviousEdgeAccessor;
		}
	}

    public partial class VoronoiMesh
    {
        [Conditional("BOUNDARY")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable UnusedParameter.Local
        private void MeshBoundaryHalfClone(Mesh oldMesh, Dictionary<Face, Face> oldToNewFace)
        // ReSharper restore UnusedParameter.Local
        {
#if BOUNDARY
            BoundaryFaces = ((BoundaryMesh)oldMesh).BoundaryFaces.Select(f => oldToNewFace[f]).ToList();
#endif
        }

        [Conditional("PREVIOUSEDGE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MeshPreviousEdgeHalfClone(Dictionary<HalfEdge, HalfEdge> oldToNewHalfEdge)
        {
            foreach (var halfEdge in HalfEdges)
            {
                (halfEdge as IPreviousEdge).PreviousEdgeAccessor = oldToNewHalfEdge[(halfEdge as IPreviousEdge).PreviousEdgeAccessor];
            }
        }

        [Conditional("PREVIOUSEDGE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MeshPreviousEdgeValidate()
        {
            if (HalfEdges.Any(edge => (edge as IPreviousEdge).PreviousEdgeAccessor == null))
            {
                throw new MeshNavException("Edge doesn't contain a previous edge");
            }
        }
    }
#endregion
}
#endregion
#endif