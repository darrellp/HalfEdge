using System.Collections.Generic;
using MeshNav;
using WE = DAP.CompGeom.WingedEdge<DAP.CompGeom.FortunePoly, DAP.CompGeom.FortuneEdge, DAP.CompGeom.FortuneVertex>;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Represents a winged edge vertex in the fortune algorithm. </summary>
	///
	/// <remarks>	Darrellp, 2/18/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public sealed class FortuneVertex : WeVertex
	{
		#region Private variables
		bool _fAlreadyOrdered;			// True after this vertex has had its edges ordered in post processing
		bool _fAddedToWingedEdge;		// True if this vertex has already been added to the winged edge data structure

		#endregion

		#region Properties
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the count of edges abutting this vertex. </summary>
		///
		/// <value>	The count of edges. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public int CtEdges => FortuneEdges.Count;

	    ////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the list of edges abutting this vertex. </summary>
		///
		/// <value>	The list of edges. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public List<FortuneEdge> FortuneEdges { get; }

		#endregion

		#region Constructors
		internal FortuneVertex(Vector pt) : base(pt)
		{
			FortuneEdges = new List<FortuneEdge>();
		}

		internal FortuneVertex()
		{
			FortuneEdges = new List<FortuneEdge>();
		}

		#endregion

		#region Polygons

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Finds the third polygon which created this vertex besides the two on each side of the passed
		/// in edge. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="edge">	Edge in question. </param>
		///
		/// <returns>	The polygon "opposite" this edge. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal FortunePoly PolyThird(FortuneEdge edge)
		{
			// Get the indices for the two polys on each side of our passed in edge
			var i1 = edge.Poly1.Index;
			var i2 = edge.Poly2.Index;

			// For each edge incident with this vertex
			foreach (var edgeDifferent in FortuneEdges)
			{
				// If it's not our own edge
				if (edgeDifferent != edge)
				{
					// The polygon we want is on one side or the other of this edge
					var i1Diff = edgeDifferent.Poly1.Index;

					// If that edge's poly1 is one of our polygons
					if (i1Diff == i1 || i1Diff == i2)
					{
						// Then we're looking for his poly2
						return edgeDifferent.Poly2;
					}
					// Otherwise, we're looking for his poly1
					return edgeDifferent.Poly1;
				}
			}
			return null;
		}
		#endregion

		#region Winged Edge
		internal void AddToWingedEdge(WE we)
		{
			if (!_fAddedToWingedEdge)
			{
				we.AddVertex(this);
				_fAddedToWingedEdge = true;
			}
		}
		#endregion

		#region Edges

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Edges are assumed to be added in a Clockwise direction.  The first edge is random and has no
		/// particular significance. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="edge">	Next clockwise edge to add. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public void Add(FortuneEdge edge)
		{
			FortuneEdges.Add(edge);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Return the point at the other end of the given edge.  If the opposite point is a point at
		/// infinity, a "real" point on that edge is created and returned. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="edge">	Edge to traverse. </param>
		///
		/// <returns>	The point at the opposite end of the edge. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		private Vector PtAtOtherEnd(WeEdge edge)
		{
			// Get the vertex at the other end
			var ptRet = VtxOtherEnd(edge).Pt;

			// If it's at infinity
			if (edge.VtxEnd.FAtInfinity)
			{
				// Produce a "real" point
				ptRet = new Vector(Pt.X + ptRet.X, Pt.Y + ptRet.Y);
			}

			// Return the result
			return ptRet;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Reset the ordered flag so we get ordered in the next call to OrderEdges() </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void ResetOrderedFlag()
		{
			_fAlreadyOrdered = false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Compare edges clockwise around this vertex. </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="e1">	The first WeEdge. </param>
		/// <param name="e2">	The second WeEdge. </param>
		///
		/// <returns>	+1 if they are CW around the generator, -1 if they're CCW, 0 if neither. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		int CompareEdges(WeEdge e1, WeEdge e2)
		{
			// Compare edges for clockwise order
			var fe1 = (FortuneEdge)e1;
			var fe2 = (FortuneEdge)e2;
			return Geometry2D.ICompareCw(Pt, fe1.PolyOrderingTestPoint, fe2.PolyOrderingTestPoint);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Order the three edges around this vertex. </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void OrderEdges()
		{
			// If this vertex has already been ordered or is a vertex at infinity
			if (_fAlreadyOrdered || FAtInfinity)
			{
				return;
			}

			// If we have the usual case of 3 edges
			if (CtEdges == 3)
			{
				// Get the points at the other end of each of the 3 edges
				var pt0 = PtAtOtherEnd(FortuneEdges[0]);
				var pt1 = PtAtOtherEnd(FortuneEdges[1]);
				var pt2 = PtAtOtherEnd(FortuneEdges[2]);

				// If ordered incorrectly
				if (Geometry2D.ICcw(pt0, pt1, pt2) > 0)
				{
					// Swap the first two
					var edge0 = FortuneEdges[0];
					FortuneEdges[0] = FortuneEdges[1];
					FortuneEdges[1] = edge0;
				}

				// Mark ourselves as ordered
				_fAlreadyOrdered = true;
			}
			else
			{
				// Do the more complicated sort
				FortuneEdges.Sort(CompareEdges);
				_fAlreadyOrdered = true;
			}
		}
		#endregion

		#region Infinite Vertices

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Produce a vertex at infinity. </summary>
		///
		/// <remarks>	Darrellp, 2/19/2011. </remarks>
		///
		/// <param name="ptDirection">	Direction for the vertex. </param>
		/// <param name="fNormalize">	If true we normalize, else not. </param>
		///
		/// <returns>	The vertex at infinity. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal static FortuneVertex InfiniteVertex(Vector ptDirection, bool fNormalize = true)
		{
			var vtx = new FortuneVertex(ptDirection);
			vtx.SetInfinite(fNormalize);
			return vtx;
		}
		#endregion
	}
}