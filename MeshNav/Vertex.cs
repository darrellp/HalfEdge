using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.TraitInterfaces;

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
    ///
    /// <typeparam name="T">    Generic type parameter for the coordinates. </typeparam>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Vertex<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Public Properties
        public Vector<T> Position { get; set; }
        public HalfEdge<T> Edge { get; internal set; }
        public int Dimension => Position.Count;
        public Mesh<T> Mesh { get; }
        #endregion

        #region Traits
        public Vector<T> Normal
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            // ReSharper disable PossibleNullReferenceException
            get => Mesh.NormalsTrait ? null : (this as INormal<T>).NormalAccessor;
            set
            {
                if (Mesh.NormalsTrait)
                {
                    (this as INormal<T>).NormalAccessor = value;
                }
            }
            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore SuspiciousTypeConversion.Global
        }

	    // ReSharper disable once InconsistentNaming
        public Vector<double> UV
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            // ReSharper disable PossibleNullReferenceException
            get => Mesh.UvTrait ? null : (this as IUV).UvAccessor;
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

        #region Constructor
        internal Vertex(Vector<T> position, Mesh<T> mesh)
        {
            Position = position;
            Mesh = mesh;
        }
		internal Vertex() { }
		#endregion

		#region Virtual functions
	    protected virtual void FillVertexTag(Vertex<T> vertex) { }
		protected virtual void FillHalfEdgeTag(HalfEdge<T> halfEdge) { }
		protected virtual void FillFaceTag(Face<T> face) { }
		#endregion

		#region Accessors
		public IEnumerable<HalfEdge<T>> AdjacentEdges()
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

        public IEnumerable<Vertex<T>> AdjacentVertices()
        {
            return AdjacentEdges().Select(adjacentEdge => adjacentEdge.NextVertex);
        }

        public IEnumerable<Face<T>> AdjacentFaces()
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
            if (!(this is INormal<T>)|| Dimension != 3)
            {
                return;
            }
            var sum = Mesh.HalfEdgeFactory.ZeroVector();
            var faceCount = 0;

            foreach (var he in AdjacentEdges())
            {
                sum = sum + FaceNormal(he);
                faceCount++;
            }
            Normal = sum.ScalarDivide(faceCount);
            Normal.Normalize(2.0);
        }

        private Vector<T> FaceNormal(HalfEdge<T> he)
        {
            var pos = Position;
            var pos1 = he.NextVertex.Position;
            var pos2 = he.NextEdge.NextVertex.Position;
            var d1 = pos1 - pos;
            var d2 = pos2 - pos;
            return Utilities.CrossProduct(d1, d2);
        }
        #endregion

        #region Mesh Operations

        public HalfEdge<T> SplitTo(Vertex<T> vtxOther, Face<T> face = null)
        {
            if (vtxOther == this)
            {
                throw new MeshNavException("Splitting with a single vertex");
            }
            if (face == null)
            {
                // We weren't given a face so figure out the face that's common between the two vertices
                face = vtxOther.AdjacentFaces().FirstOrDefault(f => !f.AtInfinity && AdjacentFaces().Contains(f));
                if (face == null)
                {
                    throw new MeshNavException("Vertices to split don't belong to a common face");
                }
            }
            else if (face.AtInfinity)
            {
                throw new MeshNavException("Can't split across the face at infinity");
            }
            HalfEdge<T> hePrevThis = null, hePrevOther = null, heNextThis = null, heNextOther = null;
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
            var heFromThis = Mesh.HalfEdgeFactory.CreateHalfEdge(this, null, face, heNextOther);
            var heFromOther = Mesh.HalfEdgeFactory.CreateHalfEdge(vtxOther, heFromThis, null, heNextThis);
	        var newFace = new Face<T>() {HalfEdge = heFromOther};
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