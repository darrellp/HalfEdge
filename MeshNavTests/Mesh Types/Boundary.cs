#pragma warning disable 1587
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
///             7. Search and replace "Bnd" with an arbitrary prefix for your project  
///             
///             After this, you should be able to build.  If you picked MyMesh for your prefix
///             then your classes will be MyMeshFactory, MyMeshMesh, etc.
///             Darrell Plank, 12/28/2017. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma warning restore 1587

#define TEMPLATE
#if TEMPLATE

// Define only ONE of the following three defines to specify how boundary conditions are handled

//#define NULLBND
#define BOUNDARY
//#define RAYED

// Pick any of the following
#define NORMALS
#define PREVIOUSEDGE
#define UV

#region Template Definition
#if (RAYED && BOUNDARY) || (RAYED && NULLBND) || (BOUNDAY && NULLBND) || (!BOUNDARY && !NULLBND && !RAYED)
#error Precisely one boundary condition should be defined in the template
#endif

#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

// ReSharper disable once RedundantUsingDirective
using MeshNav;
using MathNet.Numerics.LinearAlgebra;
// ReSharper disable RedundantUsingDirective
using MeshNav.RayedMesh;
using MeshNav.BoundaryMesh;
using MeshNav.TraitInterfaces;
// ReSharper restore RedundantUsingDirective



////////////////////////////////////////////////////////////////////////////////////////////////////
// namespace: MeshNav.TemplateFactory
//
// summary:	Rename this if desired
// 
////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Templates
{
    public class BndFactory : Factory
    {
        public BndFactory(int dimension) : base(dimension) { }

        public override Mesh CreateMesh()
        {
            return new BndMesh(Dimension, this);
        }

        public override Face CreateFace()
        {
            return new BndFace();
        }

        public override HalfEdge CreateHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
        {
            return new BndHalfEdge(vertex, opposite, face, nextEdge);
        }

        public override Vertex CreateVertex(Mesh mesh, Vector<T> vec)
        {
            return new BndVertex(mesh, vec);
        }

    }

    public class BndMesh :
#if BOUNDARY
        BoundaryMesh
#elif RAYED
        RayedMesh
#else
        Mesh
#endif
    {
        public BndMesh(int dimension, Factory factory) : base(dimension, factory) { }
    }

    public class BndFace : Face
#if BOUNDARY || RAYED
        , IBoundary
#endif
    {
#if BOUNDARY || RAYED
        public bool IsBoundaryAccessor { get; set; }
#endif
    }

    public class BndHalfEdge : HalfEdge
#if PREVIOUSEDGE
        , IPreviousEdge
#endif
    {
        public BndHalfEdge(Vertex vertex, HalfEdge opposite, Face face, HalfEdge nextEdge)
            : base(vertex, opposite, face, nextEdge)
        {
        }

#if PREVIOUSEDGE
        public HalfEdge PreviousEdgeAccessor { get; set; }
#endif
    }

    public class BndVertex : Vertex
#if NORMALS
        ,INormal
#endif
#if RAYED
        ,IRayed
#endif
#if UV
        ,IUV
#endif
    {
        internal BndVertex(Mesh mesh, Vector<T> vec) : base(mesh, vec) { }
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
}
#endregion
#endif