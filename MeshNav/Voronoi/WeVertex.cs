using System;
using System.Collections.Generic;
using System.Linq;
using MeshNav;

// ReSharper disable MemberCanBeProtected.Global

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	The vertex in a WingedEdge data structure. </summary>
	///
	/// <remarks>	
	/// In order to accommodate edges that extend off the edge of the picture, we allow for two types
	/// of vertices - normal vertices and vertices at infinity.  Vertices at infinity really
	/// represent "directions" rather than true points and are normally associated with another (non-
	/// infinite) vertex in an edge. For a point at infinity, the coordinates don't represent the
	/// true coordinates of the "point" but rather represent a normal vertex in the proper direction.
	/// The ability to represent these points at infinity as psuedo-points is crucial to making the
	/// WingedEdge structure work out since many of the edges in that structure will be between a
	/// point in the B-rep and a ray to infinity from that point. The standard WingedEdge structure
	/// makes no accommodations for such things, but we have to in the Voronoi diagram.
	/// 
	/// Darrellp, 2/18/2011. 
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public class WeVertex
	{
		#region Properties

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the first edge for enumerating edges from this vertex. </summary>
		///
		/// <value>	The first edge. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public WeEdge FirstEdge { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the polygons which contain this vertex. </summary>
		///
		/// <value>	The polygons. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<WePolygon> Polygons
		{
			get
			{
				return Edges.Select(e => ReferenceEquals(this, e.VtxStart) ? e.PolyLeft : e.PolyRight);
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the polygons which contain this vertex. </summary>
		///
		/// <value>	The polygons. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<WeEdge> Edges => new EdgeEnumerable(this);

	    ////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets a value indicating whether this is a point at infinity. </summary>
		///
		/// <value>	true if a point at infinity, false if not. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool FAtInfinity { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the location of the ver. </summary>
		///
		/// <value>	The point. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public Vector Pt { get; internal set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	A place for users to store information. </summary>
		///
		/// <value>	User specific info. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public object Cookie { get; set; }

		public Vertex HalfEdgeVertex { get; set; }
		#endregion

		#region Constructor

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Default constructor. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public WeVertex() {}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Construct a vertex from a point. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="pt">	The point the vertex is located at. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public WeVertex(Vector pt)
		{
			Pt = pt;
		}
		#endregion

		#region Infinite Vertices

		////////////////////////////////////////////////////////////////////////////////////////////////////
		///  <summary>	Make this a vertex at infinity. </summary>
		/// 
		///  <remarks>	Darrellp, 2/19/2011. </remarks>
		/// <param name="fNormalize">	true to normalize. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		protected void SetInfinite(bool fNormalize)
		{
			FAtInfinity = true;
			if (fNormalize)
			{
				Pt = Pt.Normalize();
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Convert to real physical point</summary>
		///
		/// <remarks>
		/// Points at infinity are just directions from real points.  Occasionally, though, we need a "real"
		/// point on the ray represented by the point at infinity.  This routine takes the starting point
		/// of the ray and the point at infinity and produces a point which lies on the ray.
		/// Darrellp, 2/18/2011. 
		/// </remarks>
		///
		/// <param name="ptStart">				The starting point of the ray. </param>
		/// <param name="rayLength">			Length along the ray from the starting point to our produced point. </param>
		///
		/// <returns>	A point on the ray different than the starting point. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public Vector ConvertToReal(Vector ptStart, Double rayLength)
		{
			return new Vector(
				Pt.X * rayLength + ptStart.X,
				Pt.Y * rayLength + ptStart.Y);
		}
		#endregion

		#region ToString

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/21/2011. </remarks>
		///
		/// <returns>	
		/// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. 
		/// </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return $"{(FAtInfinity ? "I" : "")}({Pt.X}, {Pt.Y})";
		}
		#endregion

		#region Modifiers

		#endregion

		#region Validation

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Ensure that the passed in edge is adjacent to this vertex. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="edge">	Edge to check. </param>
		///
		/// <returns>	True if the edge is adjacent, else false. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal bool FValidateEdgeIsAdjacent(WeEdge edge)
		{
			return Edges.Any(edgeCur => ReferenceEquals(edgeCur, edge));
		}
		#endregion

		#region Queries

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Find the vertex on the other end of an edge. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="edge">	Edge to check. </param>
		///
		/// <returns>	Vertex at the other end of the edge. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public WeVertex VtxOtherEnd(WeEdge edge)
		{
			// If we're the start vertex, then return the end and vice verse.
			return ReferenceEquals(edge.VtxStart, this) ? edge.VtxEnd : edge.VtxStart;
		}
		#endregion

		#region Internal Classes
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// WingedEdge doesn't directly give the edges which contain this vertex so we give an
		/// enumerator for that. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		class EdgeEnumerator : IEnumerator<WeEdge>
		{
			#region Private Variables

			readonly WeVertex _vtx;			// The vertex we're enumerating around
			#endregion

			#region Constructor
			internal EdgeEnumerator(WeVertex vtx)
			{
				_vtx = vtx;
			}
			#endregion

			#region IEnumerator<WEPolygon> Members

			public WeEdge Current { get; private set; }

			#endregion

			#region IDisposable Members
			public void Dispose()
			{
			}
			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current => Current;

		    public bool MoveNext()
			{
				if (Current == null)
				{
					Current = _vtx.FirstEdge;
					return Current != null;
				}
				Current = ReferenceEquals(Current.VtxStart, _vtx)
							? Current.EdgeCWPredecessor
							: Current.EdgeCWSuccessor;
				return !ReferenceEquals(Current, _vtx.FirstEdge);
			}

			public void Reset()
			{
				Current = null;
			}

			#endregion
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	The enumerable for EdgeEnumerator. </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		class EdgeEnumerable : IEnumerable<WeEdge>
		{
			#region Private Variables
			readonly WeVertex _vtx;
			#endregion

			#region Constructor
			internal EdgeEnumerable(WeVertex vtx)
			{
				_vtx = vtx;
			}
			#endregion

			#region IEnumerable<WeEdge> Members
			IEnumerator<WeEdge> IEnumerable<WeEdge>.GetEnumerator()
			{
				return new EdgeEnumerator(_vtx);
			}
			#endregion

			#region IEnumerable Members
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return new EdgeEnumerator(_vtx);
			}
			#endregion
		}
		#endregion
	}
}