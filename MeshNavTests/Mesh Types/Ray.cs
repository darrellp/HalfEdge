﻿#pragma warning disable 1587
////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A template factory. </summary>
///
/// <remarks>   To Use:
///             1. Copy to your own Build.  
///             2. Rename the file if desired   
///             3. Define TEMPLATE below  
///             4. Define exactly one boundary type  
///             5. Define whatever other traits are desired  
///             6. Rename the namespace if desired  
///             7. Search and replace "Ray" with an arbitrary prefix for your project  
///             
///             After this, you should be able to build.  If you picked MyMesh for your prefix
///             then your classes will be MyMeshFactory, MyMeshMesh, etc.
///             Darrell Plank, 12/28/2017. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma warning restore 1587

#define TEMPLATE
#if TEMPLATE

// Define only ONE of the following three defines to specify how boundary conditions are handled

//#define NULLRay
//#define BOUNDARY
#define RAYED

// Pick any of the following
#define NORMALS
#define PREVIOUSEDGE
#define UV

#region Template Definition
#if (RAYED && BOUNDARY) || (RAYED && NULLRay) || (BOUNDAY && NULLRay) || (!BOUNDARY && !NULLRay && !RAYED)
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
using MathNet.Numerics.LinearAlgebra;
using MeshNav.RayedMesh;
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
    public partial class RayFactory : Factory
    {
        public RayFactory(int dimension) : base(dimension) { }

        public override Mesh CreateMesh()
        {
            return new RayMesh(Dimension, this);
        }

        public override Face CreateFace()
        {
            return new RayFace();
        }

        public override HalfEdge CreateHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
        {
            return new RayHalfEdge(vertex, opposite, face, nextEdge);
        }

        public override Vertex CreateVertex(Mesh mesh, Vector<T> vec)
        {
            return new RayVertex(mesh, vec);
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

    public partial class RayMesh :
#if BOUNDARY
        BoundaryMesh
#elif RAYED
        RayedMesh
#else
        Mesh
#endif
    {
        public RayMesh(int dimension, Factory factory) : base(dimension, factory) { }

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

    public class RayFace : Face
#if BOUNDARY || RAYED
        , IBoundary
#endif
    {
#if BOUNDARY || RAYED
        public bool IsBoundaryAccessor { get; set; }
#endif
    }

    public class RayHalfEdge : HalfEdge
#if PREVIOUSEDGE
        , IPreviousEdge
#endif
    {
        public RayHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
            : base(vertex, opposite, face, nextEdge)
        {
        }

#if PREVIOUSEDGE
        public HalfEdge PreviousEdgeAccessor { get; set; }
#endif
    }

    public class RayVertex : Vertex
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
        internal RayVertex(Mesh mesh, Vector<T> vec) : base(mesh, vec) { }
#if RAYED
        public bool IsRayed { get; set; }
#endif

#if NORMALS
        public Vector<T> NormalAccessor { get; set; }
#endif

#if UV
        public Vector<T> UvAccessor { get; set; }
#endif
    }

    #region Conditionals
    public partial class RayFactory
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

    public partial class RayMesh
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