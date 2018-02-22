using MeshNav;
using WE = DAP.CompGeom.WingedEdge<DAP.CompGeom.FortunePoly, DAP.CompGeom.FortuneEdge, DAP.CompGeom.FortuneVertex>;
using static MeshNav.Geometry2D;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Fortune edge. </summary>
	///
	/// <remarks>
	/// An edge for a winged edge structure but specifically designed for the voronoi algorithm.
	/// Darrellp, 2/18/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public class FortuneEdge : WeEdge
	{
		#region Private Variables
		bool _fStartVertexSet;										// True if the first vertex has already been set on this edge
		bool _fAddedToWingedEdge;									// True if we've already been added to a winged edge data structure
		readonly FortunePoly[] _arPoly = new FortunePoly[2];		// The polygons on each side of us
		#endregion

		#region Properties
		internal FortunePoly Poly1 => _arPoly[0];
	    internal FortunePoly Poly2 => _arPoly[1];

	    ////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets a value indicating whether this is part of a split doubly infinite edge. </summary>
		///
		/// <value>	true if split, false if not. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool FSplit { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Return a point suitable for testing angle around the generator. </summary>
		/// 
		/// <remarks>	
		/// When we order the edges in CW or CCW order, we need to get representative points on each edge
		/// so that we can compare the angle they make with the vertex being tested.  This means they
		/// have to be on the edge but not at either vertex so that we can order the edges of polygons in
		/// postprocessing.  This is used in the CompareToVirtual() to effect that ordering. 
		/// </remarks>
		///
		/// <value>	The polygon ordering test point. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal Vector PolyOrderingTestPoint
		{
			get
			{
				// Finite line segments just take the midpoint
				if (!VtxEnd.FAtInfinity)
				{
					return MidPoint(VtxStart.Pt, VtxEnd.Pt);
				}

				// If it's a ray
				if (!VtxStart.FAtInfinity)
				{
					// Just take a point a little out from the start point
					return new Vector(
						VtxStart.Pt.X + VtxEnd.Pt.X,
						VtxStart.Pt.Y + VtxEnd.Pt.Y);
				}

				// Take care of an edge at infinity
				//
				// We use the midpoint of the two starting points of the rays on
				// each side of the edge at infinity
				return MidPoint(
					EdgeCCWSuccessor.VtxStart.Pt,
					EdgeCWPredecessor.VtxStart.Pt);
			}
		}
		#endregion

		#region ToString

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Convert this object into a string representation. </summary>
		///
		/// <remarks>	Darrellp, 2/21/2011. </remarks>
		///
		/// <returns>	A string representation of this object. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return $"{base.ToString()} : Gens {_arPoly[0].Index} - {_arPoly[1].Index}:";
		}
		#endregion

		#region Queries

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Is this edge zero length?  Infinite rays are never zero length. </summary>
		///
		/// <remarks>	Darrellp, 2/21/2011. </remarks>
		///
		/// <returns>	true if the edge is zero length. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal bool FZeroLength()
		{
			return !VtxEnd.FAtInfinity && FCloseEnough(VtxStart.Pt, VtxEnd.Pt);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Find the index of this edge in an adjacent vertex. </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="fStartVertex">	If true, search start vertex, else search end vertex. </param>
		///
		/// <returns>	Index in the vertice's edge list of this edge. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal int EdgeIndex(bool fStartVertex)
		{
			// Get the vertex we're supposed to search
			var vtx = (FortuneVertex)(fStartVertex ? VtxStart : VtxEnd);

			// Go through all the edges for that vertex
			for (var iEdge = 0; iEdge < vtx.FortuneEdges.Count; iEdge++)
			{
				// When we locate this edge
				if (vtx.FortuneEdges[iEdge] == this)
				{
					// Return the index
					return iEdge;
				}
			}
			// We should never make it to here
			return -1;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Returns the polygon on the other side of this edge. </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="polyThis">	The polygon on "this" side. </param>
		///
		/// <returns>	The polygon on the "other" side. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal FortunePoly OtherPoly(FortunePoly polyThis)
		{
			return Poly1 == polyThis ? Poly2 : Poly1;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Determine whether two edges connect at a common vertex and if so, how they connect. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="edge1">					First edge. </param>
		/// <param name="edge2">					Second edge. </param>
		/// <param name="fEdge1ConnectsAtStartVtx">	[out] True if edge1 connects to edge2 at its start
		/// 										vertex, else false. </param>
		/// <param name="fEdge2ConnectsAtStartVtx">	[out] True if edge2 connects to edge1 at its start
		/// 										vertex, else false. </param>
		///
		/// <returns>	true if the edges connect. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal static bool FConnectsTo(
			FortuneEdge edge1,
			FortuneEdge edge2,
			out bool fEdge1ConnectsAtStartVtx,
			out bool fEdge2ConnectsAtStartVtx)
		{
			// Locals
			var fRet = false;

			// Init out parameters to false
			fEdge1ConnectsAtStartVtx = false;
			fEdge2ConnectsAtStartVtx = false;

			// RQS- Compare starting and ending vertices
			if (ReferenceEquals(edge1.VtxStart, edge2.VtxStart))
			{
				fEdge1ConnectsAtStartVtx = true;
				fEdge2ConnectsAtStartVtx = true;
				fRet = true;
			}
			else if(ReferenceEquals(edge1.VtxStart, edge2.VtxEnd))
			{
				fEdge1ConnectsAtStartVtx = true;
				fRet = true;
			}
			else if (ReferenceEquals(edge1.VtxEnd, edge2.VtxStart))
			{
				fEdge2ConnectsAtStartVtx = true;
				fRet = true;
			}
			else if (ReferenceEquals(edge1.VtxEnd, edge2.VtxEnd))
			{
				fRet = true;
			}
			//-RQS

			// Return the result
			return fRet;
		}
		#endregion

		#region Modification

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Relabel all the end vertices of this edge to point to it's start vertex. </summary>
		/// 
		/// <remarks>
		/// This is done only for zero length edges when they're deleted.  The edges emanating from the
		/// end vertex of the zero length edge are moved to the start vertex (which is in the same location
		/// as the end vertex).  Since these will not be ordered with the edges already there we set them to
		/// unordered.
		/// 	
		/// Darrellp, 2/19/2011.
		/// </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		private void RelabelEndVerticesToStart()
		{
			// For each edge emanating at the end vertex
			//
			// We HAVE to use FortuneEdges here since the WeVertex.Edges enumeration depends on
			// CW and CCW predecessors which we're in the process of messing around with - a
			// recipe for disaster!
			foreach (var edgeCur in ((FortuneVertex)VtxEnd).FortuneEdges)
			{
				// If it's not the zero length edge
				if (!ReferenceEquals(edgeCur, this))
				{
					// If it connects with it's Start vertex
					if (edgeCur.VtxStart == VtxEnd)
					{
						// reassign its start vertex
						edgeCur.VtxStart = VtxStart;
					}
					else
					{
						// reassign its end vertex
						edgeCur.VtxEnd = VtxStart;
					}
				}
			}
			
			// Reset the ordering flag on the start vertex
			((FortuneVertex)VtxStart).ResetOrderedFlag();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Splice all the end vertices of this edge into the edge list of it's start vertex. </summary>
		///
		/// <remarks>	We do this when removing zero length edges. Darrellp, 2/19/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		private void SpliceEndEdgesIntoStart()
		{
			// Initialize locals
			var iEnd = EdgeIndex(false);
			var iStart = EdgeIndex(true);
			var lstSpliceInto = ((FortuneVertex)VtxStart).FortuneEdges;
			var lstSpliceFrom = ((FortuneVertex)VtxEnd).FortuneEdges;

			// Now add all our end vertices to the start vertex's edge list.
			// 
			// We add them in reverse order starting from the edge before
			// this one so that they end up in proper order in the target list
			for (var i = (iEnd + lstSpliceFrom.Count - 1) % lstSpliceFrom.Count; i != iEnd; i = (i + lstSpliceFrom.Count - 1) % lstSpliceFrom.Count)
			{
				// Put them into the start list
				lstSpliceInto.Insert(iStart, lstSpliceFrom[i]);
			}

			// RQS- Take care of CW Predecessor
			if (EdgeCWPredecessor.VtxEnd == VtxStart)
			{
				EdgeCWPredecessor.EdgeCCWSuccessor = EdgeCCWSuccessor;
			}
			else
			{
				EdgeCWPredecessor.EdgeCCWPredecessor = EdgeCCWSuccessor;
			}
			//-RQS

			// RQS- and CCW Predecessor
			if (EdgeCCWPredecessor.VtxEnd == VtxStart)
			{
				EdgeCCWPredecessor.EdgeCWSuccessor = EdgeCWSuccessor;
			}
			else
			{
				EdgeCCWPredecessor.EdgeCWPredecessor = EdgeCWSuccessor;
			}
			//-RQS

			// RQS- and CW Successor
			if (EdgeCWSuccessor.VtxEnd == VtxEnd)
			{
				EdgeCWSuccessor.EdgeCCWSuccessor = EdgeCCWPredecessor;
			}
			else
			{
				EdgeCWSuccessor.EdgeCCWPredecessor = EdgeCCWPredecessor;
			}
			//-RQS

			// RQS- and CCW Successor
			if (EdgeCCWSuccessor.VtxEnd == VtxEnd)
			{
				EdgeCCWSuccessor.EdgeCWSuccessor = EdgeCWPredecessor;
			}
			else
			{
				EdgeCCWSuccessor.EdgeCWPredecessor = EdgeCWPredecessor;
			}
			//-RQS

			// Remove us from the start vertex's list
			//
			// Our index has been bumped by all the edges
			// we inserted which is just all the ones from the end vertex minus 1 for ourself.
			lstSpliceInto.RemoveAt(iStart + lstSpliceFrom.Count - 1);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Move all the edges on the end vertex to the start. </summary>
		///
		/// <remarks>	
		/// This is done to removing a zero length edge.  Since the edge is zero length
		/// we can assume that all the end edges fit in the "wedge" occupied by this edge in the start
		/// vertices list of edges.  That is, we can assume that we can just splice the end edges into
		/// the start vertex's list of edges without having to resort based on angle. 
		/// </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void ReassignVertexEdges()
		{
			// Put the end edges into our start vertex's list of edges
			SpliceEndEdgesIntoStart();

			// Make all the end edges join to our start vertex rather than our end vertex
			RelabelEndVerticesToStart();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Add a vertex in the proper place according to _fStartVertexSet. </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="vtx">	Vertex to add. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void AddVertex(FortuneVertex vtx)
		{
			// If we've already set the start vertex
			if (_fStartVertexSet)
			{
				// This is the end vertex
				VtxEnd = vtx;
			}
			else
			{
				// Make this the start vertex and set the start vertex flag
				_fStartVertexSet = true;
				VtxStart = vtx;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Get the next edge in both the cw and ccw directions from this edge at the given vertex. 
		/// </summary>
		///
		/// <remarks>	
		/// This routine is called before they've been set up as winged edges so we have to search them
		/// out ourselves.  The edges have been ordered in CW order, however.
		/// 
		/// Darrellp, 2/19/2011. 
		/// </remarks>
		///
		/// <param name="vtx">		vertex to use. </param>
		/// <param name="edgeCW">	[out] returned cw edge. </param>
		/// <param name="edgeCCW">	[out] returned ccw edge. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		void GetSuccessorEdgesFromVertex(
			FortuneVertex vtx,
			out FortuneEdge edgeCW,
			out FortuneEdge edgeCCW)
		{
			// Are we free of zero length edges?
			if (vtx.FortuneEdges.Count == 3)
			{
				// Do the extremely simple, extremely common 3 valent case
				GetSuccessorEdgesFrom3ValentVertex(vtx, out edgeCW, out edgeCCW);
			}
			else
			{
				// Locals
				int iEdge;
				int cEdges;

				// If we're looking at our start vertex
				if (vtx == VtxStart)
				{
					// Find our place in the list of edges for start vertex
					iEdge = EdgeIndex(true);
					cEdges = ((FortuneVertex)VtxStart).CtEdges;
				}
				else
				{
					// Find our place in the list of edges for end vertex
					iEdge = EdgeIndex(false);
					cEdges = ((FortuneVertex)VtxEnd).CtEdges;
				}

				// Get our immediate neighbors on the edge list
				edgeCW = vtx.FortuneEdges[(iEdge + 1) % cEdges];
				edgeCCW = vtx.FortuneEdges[(iEdge + cEdges - 1) % cEdges];
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Get the next edge in both the cw and ccw directions from this edge at the given 3 valent
		/// vertex. 
		/// </summary>
		///
		/// <remarks>
		/// This is the simplest and most common case.  It's called before the winged edge stuff is set up
		/// so we have to search manually.  The edges have been sorted in clockwise order however.
		/// 
		/// Darrellp, 2/19/2011.
		/// </remarks>
		///
		/// <param name="vtx">		vertex to use. </param>
		/// <param name="edgeCW">	[out] returned cw edge. </param>
		/// <param name="edgeCCW">	[out] returned ccw edge. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		void GetSuccessorEdgesFrom3ValentVertex(
			FortuneVertex vtx,
			out FortuneEdge edgeCW,
			out FortuneEdge edgeCCW)
		{
			// Locals
			int iEdge;

			// Figure out which of the edges is ours
			for (iEdge = 0; iEdge < 3; iEdge++)
			{
				// If the current edge is us
				if (ReferenceEquals(vtx.FortuneEdges[iEdge], this))
				{
					break;
				}
			}

			// The next two edges, in order, are the onew we're looking for
			edgeCW = vtx.FortuneEdges[(iEdge + 1) % 3];
			edgeCCW = vtx.FortuneEdges[(iEdge + 2) % 3];
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Place the poly on the proper side of this edge.  We use the generator of the poly to locate
		/// it properly WRT this edge. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="poly">	Polygon to locate. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void SetOrderedPoly(FortunePoly poly)
		{
			if (FLeftOf(poly.VoronoiPoint))
			{
				PolyLeft = poly;
			}
			else
			{
				PolyRight = poly;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Set up the "wings" for this edge - i.e., it's successor edges in both cw and ccw directions
		/// at both start and end vertices. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void SetSuccessorEdges()
		{
			// Locals
		    var fvtx = ((FortuneVertex) VtxStart);

			// If this is the degenerate case of a fully infinite line
			//
			// With exactly collinear points in the voronoi diagram we would
			// normally have a fully infinite line(s).  This is the only situation
			// where this arises and we handle it by turning it into two
			// rays pointing in opposite directions.  If we're one of those
			// edges, then both our predecessor and our successor are the
			// other edge.
			if (fvtx.FortuneEdges.Count == 2)
			{
				// If we're Edges[0]
				if (ReferenceEquals(this, fvtx.FortuneEdges[0]))
				{
					// Set our pred and succ to Edges[1]
					EdgeCCWPredecessor = EdgeCWPredecessor = fvtx.FortuneEdges[1];
				}
				else
				{
					// Set our pred ans succ to Edges[0]
					EdgeCCWPredecessor = EdgeCWPredecessor = fvtx.FortuneEdges[0];
				}
				return;
			}

			// Sort the edges around the vertex
			//
			// We can't keep edges sorted during the sweepline processing so we do it here in
			// postprocessing
			((FortuneVertex)VtxStart).OrderEdges();

			// Get our predecessor edges
			GetSuccessorEdgesFromVertex(
				((FortuneVertex)VtxStart),
				out var edgeCW,
				out var edgeCCW);
			EdgeCWPredecessor = edgeCW;
			EdgeCCWPredecessor = edgeCCW;

			// If the end vertex is not at infinity, set up successor edges
			//
			// If one end is an infinite ray then it's successors will be set up when we add the polygon
			// at infinity on.
			if (!VtxEnd.FAtInfinity)
			{
				// Order our end vertices
				((FortuneVertex)VtxEnd).OrderEdges();

				// Get our successor edges
				GetSuccessorEdgesFromVertex(
					((FortuneVertex)VtxEnd),
					out edgeCW,
					out edgeCCW);
				EdgeCWSuccessor = edgeCW;
				EdgeCCWSuccessor = edgeCCW;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Add a poly to the edge and the edge to the winged edge data structure. </summary>
		///
		/// <remarks>	
		/// This is where we set up the CW and CCW successor and predecessor edges, the polygons on each
		/// side of the edge, it's start and end vertices.  Also, the edge is added to to start and end
		/// vertices' edge list and it's actually added to the winged edge structure itself
		/// 
		/// Darrellp, 2/18/2011. 
		/// </remarks>
		///
		/// <param name="poly">	Polygon to add. </param>
		/// <param name="we">	Winged edge structure to add edge to. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void HookToWingedEdge(FortunePoly poly, WE we)
		{
			// Put the poly properly to the left or right of this edge
			SetOrderedPoly(poly);

			// Avoid adding the edge twice
			//
			// We'll attempt to add this edge twice, once from each polygon on each side of
			// it.  We only want to add it one of those times.
			if (!_fAddedToWingedEdge)
			{
				// Set up the successor edges
				SetSuccessorEdges();

				we.AddEdge(this);
				_fAddedToWingedEdge = true;

				// Set up the start and end vertex
				VtxStart.FirstEdge = this;
				VtxEnd.FirstEdge = this;
				((FortuneVertex) VtxStart).AddToWingedEdge(we);
				((FortuneVertex) VtxEnd).AddToWingedEdge(we);
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Set up the polygons which surround this edge. </summary>
		///
		/// <remarks>	
		/// During the sweepline processing we don't necessarily know where the final vertex for an edge
		/// will be before we know the polygons on each side of the edge so we can't actually determine
		/// which side of the edge the polygons will lie on.  Consequently, we have to just keep them
		/// handy until we finally get our second point.
		/// 
		/// Darrellp, 2/19/2011. 
		/// </remarks>
		///
		/// <param name="poly1">	First poly. </param>
		/// <param name="poly2">	Second poly. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void SetPolys(FortunePoly poly1, FortunePoly poly2)
		{
			_arPoly[0] = poly1;
			_arPoly[1] = poly2;
		}
		#endregion

		#region IComparable Members

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Find the common polygon between two edges.  An assertion will be raised if there is no common
		/// polygon. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="edge">	Edge to find a common poly with. </param>
		///
		/// <returns>	The common polygon. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		private FortunePoly PolyCommon(FortuneEdge edge)
		{
			// If my Poly1 is the same as one of the edge's polys
			if (ReferenceEquals(Poly1, edge.Poly1) || ReferenceEquals(Poly1, edge.Poly2))
			{
				// Return poly1
				return Poly1;
			}

			// Otherwise, return Poly2
			return Poly2;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Compare two edges based on their cw order around a common generator. </summary>
		///
		/// <remarks>	
		/// It is an error to compare edges which do not have a common generator so this is only a
		/// "partial" comparer which is probably strictly verboten according to C# rules, but we have to
		/// do it in order to use the framework Sort routine to sort edges around a generator. 	
		/// 
		/// Darrellp, 2/19/2011. 
		/// </remarks>
		///
		/// <param name="edgeIn">	Edge to compare. </param>
		///
		/// <returns>	Comparison output. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal override int CompareToVirtual(WeEdge edgeIn)
		{
			// Get our fortune edge
			var edge = edgeIn as FortuneEdge;

			// If they're identical
			if (ReferenceEquals(edgeIn, this))
			{
				return 0;
			}

			// Return the "clockwisedness" of the generator and points on the two edges.
			// ReSharper disable once PossibleNullReferenceException
			return ICompareCw(PolyCommon(edge).VoronoiPoint, PolyOrderingTestPoint, edge.PolyOrderingTestPoint);
		}
		#endregion
	}
}