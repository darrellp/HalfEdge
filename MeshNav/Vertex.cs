using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.TraitInterfaces;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A vertex. </summary>
    ///
    /// <remarks>   Vertices in the halfedge data structure contain two pieces of information - the
    ///             actual position of the vertex and a halfedge emanating from the vertex.  We also
    ///             add an object Tag for the user to place custom values in.  Tag also serves as
    ///             a repository for various traits which are expressed as interfaces which the Tag
    ///             may or may not support.  For instance, if the tag supports the INormal interface
    ///             then we can place normals on this vertex.
    ///             Darrell Plank, 12/9/2017. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Vertex : MeshComponent
    {
        #region Public Properties
        public Vector<T> Position { get; set; }
        public HalfEdge Edge { get; internal set; }
        public int Dimension => Position.Count;
        public Mesh Mesh { get; }

        public T X
        {
            get => this[0];
            set => this[0] = value;
        }

        public T Y
        {
            get => this[1];
            set => this[1] = value;
        }

        public T Z
        {
            get => this[2];
            set => this[2] = value;
        }
        #endregion

        #region Traits
        public Vector<T> Normal
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            // ReSharper disable PossibleNullReferenceException
            get => Mesh.NormalsTrait ? (this as INormal).NormalAccessor : null;
            set
            {
                if (Mesh.NormalsTrait)
                {
                    (this as INormal).NormalAccessor = value;
                }
            }
            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore SuspiciousTypeConversion.Global
        }

	    // ReSharper disable once InconsistentNaming
        public Vector<T> UV
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            // ReSharper disable PossibleNullReferenceException
            get => Mesh.UvTrait ? (this as IUV).UvAccessor : null;
            set
            {
                if (Mesh.UvTrait)
                {
                    (this as IUV).UvAccessor = value;
                }
            }
            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore SuspiciousTypeConversion.Global
        }
        #endregion

        #region Operator overrides
        public T this[int index]
        {
            get => Position[index];
            set => Position[index] = value;
        }
        #endregion

        #region Constructor
        internal Vertex(Mesh mesh, Vector<T> position)
        {
            Position = position;
            Mesh = mesh;
        }
		internal Vertex() { }
		#endregion

		#region Virtual functions
	    protected virtual void FillVertexTag(Vertex vertex) { }
		protected virtual void FillHalfEdgeTag(HalfEdge halfEdge) { }
		protected virtual void FillFaceTag(Face face) { }
		#endregion

		#region Accessors
		public IEnumerable<HalfEdge> AdjacentEdges()
        {
            if (Edge == null)
            {
                yield break;
            }
            var curEdge = Edge;
            do
            {
                yield return curEdge;
                if (curEdge.Opposite == null)
                {
                    throw new MeshNavException("Incomplete Mesh");
                }
                curEdge = curEdge.Opposite.NextEdge;
                if (curEdge == null)
                {
                    throw new MeshNavException("Incomplete Mesh");
                }
            } while (curEdge != Edge);
        }

        public IEnumerable<Vertex> AdjacentVertices()
        {
            return AdjacentEdges().Select(adjacentEdge => adjacentEdge.NextVertex);
        }

        public IEnumerable<Face> AdjacentFaces()
        {
            return AdjacentEdges().Select(adjacentEdge => adjacentEdge.Face);
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            var sb = new StringBuilder("(");
            for(var iDim = 0; iDim < Dimension; iDim++)
            {
                var tag = iDim == Dimension - 1 ? ")" : ", ";
                sb.Append($"{Position[iDim]}{tag}");
            }
            return sb.ToString();
        }
        #endregion

        #region Traits
        internal void CalculateNormal()
        {
            if (!(this is INormal)|| Dimension != 3)
            {
                return;
            }
            var sum = Mesh.Factory.ZeroVector();
            var faceCount = 0;

            foreach (var he in AdjacentEdges())
            {
                sum = sum + FaceNormal(he);
                faceCount++;
            }
            Normal = sum / faceCount;
            Normal.Normalize(2.0);
        }

        private Vector<T> FaceNormal(HalfEdge he)
        {
            var pos = Position;
            var pos1 = he.NextVertex.Position;
            var pos2 = he.NextEdge.NextVertex.Position;
            var d1 = pos1 - pos;
            var d2 = pos2 - pos;
            return CrossProduct(d1, d2);
        }
        #endregion

        #region Mesh Operations

        public HalfEdge SplitTo(Vertex vtxOther, Face face = null)
        {
            if (vtxOther == this)
            {
                throw new MeshNavException("Splitting with a single vertex");
            }
            if (face == null)
            {
                // We weren't given a face so figure out the face that's common between the two vertices
                face = vtxOther.AdjacentFaces().FirstOrDefault(f => !f.IsBoundary && AdjacentFaces().Contains(f));
                if (face == null)
                {
                    throw new MeshNavException("Vertices to split don't belong to a common face");
                }
            }
            else if (face.IsBoundary)
            {
                throw new MeshNavException("Can't split across the face at infinity");
            }
            HalfEdge hePrevThis = null, hePrevOther = null, heNextThis = null, heNextOther = null;
            foreach (var halfEdge in face.Edges())
            {

                if (halfEdge.InitVertex == this)
                {
                    heNextThis = halfEdge;
                    if (heNextThis.NextVertex == vtxOther)
                    {
                        throw new MeshNavException("Can't split between adjacent vertices");
                    }
                }
                if (halfEdge.NextVertex == this)
                {
                    hePrevThis = halfEdge;
                }
                if (halfEdge.InitVertex == vtxOther)
                {
                    heNextOther = halfEdge;
                    if (heNextOther.NextVertex == this)
                    {
                        throw new MeshNavException("Can't split between adjacent vertices");
                    }
                }
                if (halfEdge.NextVertex == vtxOther)
                {
                    hePrevOther = halfEdge;
                }
            }
            if (hePrevOther == null || hePrevThis == null)
            {
                throw new MeshNavException("Vertices to split don't belong to given face");
            }
            var heFromThis = Mesh.Factory.CreateHalfEdge(this, null, face, heNextOther);
            var heFromOther = Mesh.Factory.CreateHalfEdge(vtxOther, heFromThis, null, heNextThis);
	        var newFace = new Face() {HalfEdge = heFromOther};
	        heFromOther.Face = newFace;
            heFromThis.Opposite = heFromOther;
            hePrevThis.NextEdge = heFromThis;
            hePrevOther.NextEdge = heFromOther;
            face.HalfEdge = heFromThis;

            Mesh.FacesInternal.Add(face);
            Mesh.HalfEdgesInternal.Add(heFromThis);
            Mesh.HalfEdgesInternal.Add(heFromOther);
            return null;
        }
        #endregion
    }
}