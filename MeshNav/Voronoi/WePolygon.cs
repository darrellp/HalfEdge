using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MeshNav;

// ReSharper disable MemberCanBeProtected.Global

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Polygons in a WingedEdge structure. </summary>
	/// <remarks>
	///     <para>
	///         Essentially a pointer to a random edge in this polygon.  The other edges can be
	///         navigated to by following predecessor and successor fields in the edges.  An enumerator is
	///         supplied which does this for you.  We also supply one for the vertices.
	///     </para>
	///     Darrellp, 2/18/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////
	public class WePolygon
	{
		#region Properties
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	A place for users to store information. </summary>
		/// <value>	User specific info. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public object Cookie { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the first edge of an enumeration. </summary>
		/// <value>	The first edge in an enumeration. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal WeEdge FirstEdge { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the edges in clockwise order. </summary>
		/// <value>	The edges. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<WeEdge> Edges => new EdgeEnumerable(this);

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets oriented edges which gives the edge along with the direction to move on it. </summary>
		/// <value>	The oriented edges. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<OrientedEdge> OrientedEdges
		{
			get { return Edges.Select(e => new OrientedEdge(e, ReferenceEquals(e.PolyLeft, this))); }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the vertices in clockwise order. </summary>
		/// <value>	The vertices. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<WeVertex> Vertices
		{
			get { return Edges.Select(e => ReferenceEquals(e.PolyLeft, this) ? e.VtxStart : e.VtxEnd); }
		}
		#endregion

		#region Validation
		/// <summary>
		///     Place to set a breakpoint to detect failure in the validation routines
		/// </summary>
		/// <returns></returns>
		private static bool Failure()
		{
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Ensure that a given edge is adjacent to this polygon. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		/// <param name="edge">	Edge to be checked. </param>
		/// <returns>	True if the edge is adjacent, else false. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal bool FValidateEdgeIsAdjacent(WeEdge edge)
		{
			return edge.PolyLeft == this || edge.PolyRight == this;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Ensure that all edges connect to each other. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		/// <returns>	True if all edges connect in order, else false. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal bool FValidateEdgesInOrder()
		{
			// If it's a single infinite polygon
			if (!Edges.Any())
			{
				// There are no edges so skip this check
				return true;
			}

			// Declarations
			var fFirstTimeThroughLoop = true;
			var edgePrev = new WeEdge();
			var edgeFirst = new WeEdge();

			// For each edge in the polygon
			foreach (var edgeCur in Edges)
			{
				if (fFirstTimeThroughLoop)
				{
					// Initialize
					fFirstTimeThroughLoop = false;
					edgePrev = edgeFirst = edgeCur;
				}
				else
				{
					// If this edge doesn't connect to the previous one
					if (!edgeCur.FConnectsToEdge(edgePrev))
					{
						// there is a problem
						return Failure();
					}

					edgePrev = edgeCur;
				}
			}

			// Make sure the last edge cycles back to the first one
			return edgePrev.FConnectsToEdge(edgeFirst) || Failure();
		}
		#endregion

		#region Internal Classes
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     Gives an edge along with an orientation showing whether it starts at the actual StartVertex
		///     or is reversed and starts at the EndVertex.  I wouldn't have to do this if I'd have used the
		///     half edge structure rather than winged edge.  Live and learn.
		/// </summary>
		/// <remarks>	Darrellp, 2/22/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public struct OrientedEdge
		{
			internal OrientedEdge(WeEdge edge, bool fForward)
			{
				Forward = fForward;
				Edge = edge;
			}

			/// <summary>
			///     True if the edge travels from the StartVertex to the EndVertex.  If false
			///     then we should traverse the edge from the EndVertex to the StartVertex.
			/// </summary>
			public readonly bool Forward;

			/// <summary> The edge in question </summary>
			public readonly WeEdge Edge;

			////////////////////////////////////////////////////////////////////////////////////////////////////
			/// <summary>	Gets the start vertex. </summary>
			/// <value>	The start vertex. </value>
			////////////////////////////////////////////////////////////////////////////////////////////////////

			public WeVertex StartVtx => Forward ? Edge.VtxStart : Edge.VtxEnd;

			////////////////////////////////////////////////////////////////////////////////////////////////////
			/// <summary>	Gets the end vertex. </summary>
			/// <value>	The end vertex. </value>
			////////////////////////////////////////////////////////////////////////////////////////////////////

			public WeVertex EndVtx => !Forward ? Edge.VtxStart : Edge.VtxEnd;

			////////////////////////////////////////////////////////////////////////////////////////////////////
			/// <summary>	Gets the start point for this directed edge. </summary>
			/// <value>	The start point. </value>
			////////////////////////////////////////////////////////////////////////////////////////////////////

			public Vector StartPt => StartVtx.Pt;

			////////////////////////////////////////////////////////////////////////////////////////////////////
			/// <summary>	Gets the end point for this directed edge. </summary>
			/// <value>	The end point. </value>
			////////////////////////////////////////////////////////////////////////////////////////////////////

			public Vector EndPt => EndVtx.Pt;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     The standard WingedEdge doesn't directly give the edges around a polygon so this is an
		///     enumerator for that. They are returned in CCW order.
		/// </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		private class EdgeEnumerator : IEnumerator<WeEdge>
		{
			#region Private Variables
			private readonly WePolygon _poly; // Polygon whose vertices we're enumerating
			#endregion

			#region Constructor
			internal EdgeEnumerator(WePolygon poly)
			{
				_poly = poly;
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
			object IEnumerator.Current => Current;

			public bool MoveNext()
			{
				// If this is the first call
				if (Current == null)
				{
					// Use the first edge
					Current = _poly.FirstEdge;

					// Return true only if there's an edge in the polygon at all
					return Current != null;
				}

				// If not our first call, just get the next edge until we've looped back
				Current = ReferenceEquals(Current.PolyLeft, _poly) ? Current.EdgeCWSuccessor : Current.EdgeCWPredecessor;
				return !ReferenceEquals(Current, _poly.FirstEdge);
			}

			public void Reset()
			{
				Current = null;
			}
			#endregion
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	An enumerator for the edges of a polygon in CCW order. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		private class EdgeEnumerable : IEnumerable<WeEdge>
		{
			#region Private Variables
			private readonly WePolygon _poly;
			#endregion

			#region Constructor
			internal EdgeEnumerable(WePolygon poly)
			{
				_poly = poly;
			}
			#endregion

			#region IEnumerable<WeEdge> Members
			IEnumerator<WeEdge> IEnumerable<WeEdge>.GetEnumerator()
			{
				return new EdgeEnumerator(_poly);
			}
			#endregion

			#region IEnumerable Members
			IEnumerator IEnumerable.GetEnumerator()
			{
				return new EdgeEnumerator(_poly);
			}
			#endregion
		}
		#endregion
	}
}