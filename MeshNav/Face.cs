using System;
using System.Collections.Generic;
using System.Linq;
using MeshNav.TraitInterfaces;

namespace MeshNav
{
    public class Face<T> where T : struct, IEquatable<T>, IFormattable
    {
        #region Public Properties
        public HalfEdge<T> HalfEdge { get; internal set; }
	    public Mesh<T> Mesh => HalfEdge.Mesh;
        #endregion

        #region Traits
        public bool AtInfinity
        {
            // ReSharper disable PossibleNullReferenceException
            get => !Mesh.AtInfinityTrait && (this as IAtInfinity).AtInfinityAccessor;
            set
            {
                if (Mesh.AtInfinityTrait)
                {
                    (this as IAtInfinity).AtInfinityAccessor = value;
                }
            }
            // ReSharper restore PossibleNullReferenceException
        }
        #endregion

        #region Constructor
        internal Face() {}
        #endregion

        #region Accessors
        public IEnumerable<HalfEdge<T>> Edges()
        {
            if (HalfEdge == null)
            {
                // This occurs on the FaceAtInfinity when there are no border edges
                yield break;
            }
            var curEdge = HalfEdge;
            do
            {
                yield return curEdge;
                curEdge = curEdge.NextEdge;
            } while (curEdge != HalfEdge);
        }

        public IEnumerable<Vertex<T>> Vertices()
        {
            return Edges().Select(e => e.InitVertex);
        }
        #endregion

        #region Validation
        internal bool Validate()
        {
            var edges = new HashSet<HalfEdge<T>>();
            var lastEdge = (HalfEdge<T>)null;
            foreach (var halfEdge in Edges())
            {
                if (edges.Contains(halfEdge))
                {
                    throw new MeshNavException("Edges loop back on themselves");
                }
                if (lastEdge != null && lastEdge.NextEdge != halfEdge)
                {
                    throw new MeshNavException("Improper edge linkage within face");
                }
                lastEdge = halfEdge;
                edges.Add(halfEdge);
            }
            return true;
        }
        #endregion
    }
}