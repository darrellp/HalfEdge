using System.Text;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Leaf node in the tree representation of our beachline. </summary>
	///
	/// <remarks>
	/// In the beachline tree, leaf nodes represent individual polygons.  As such, they point at the
	/// FortunePoly which will eventually be placed in the winged edge rep.  The are removed when
	/// the parabolas for their neighbors become wide enough to eliminate them.  This is represented by
	/// a circle event on the queue.  We keep track of the circle event that pertains to our demise.
	/// Kinda morbid - I know.
	/// 
	/// We also keep pointers to our left siblings and our right siblings.  One of these siblings will
	/// be an "immediate" sibling - i.e., the other sibling of our parent.  The other sibling will be
	/// further off and harder to find so keeping these pointers to both siblings speed things up.
	/// 
	/// Keeping in mind the equivalence between the beachline, the tree representing the beachline and the
	/// evolving elements in the voronoi diagram is a big key to understanding this code and the
	/// Fortune algorithm in general.
	/// 
	/// Darrellp, 2/18/2011. 
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	internal class LeafNode : Node
	{
		#region Private Variables

		CircleEvent _cevt;						// Circle event which will snuff this parabola out

		#endregion

		#region Properties
		// Polygon being created by this node
		internal FortunePoly Poly { get; set; }

		// Node for the parabola to our right
		internal LeafNode RightAdjacentLeaf { get; set; }

		// Node for the parabola to our left
		internal LeafNode LeftAdjacentLeaf { get; set; }
		#endregion

		#region Constructor
		internal LeafNode(FortunePoly poly)
		{
			LeftAdjacentLeaf = null;
			RightAdjacentLeaf = null;
			Poly = poly;
		}

		#endregion

		#region ToString
		public override string ToString()
		{
			return $"LeafNode: Gen = {Poly.Index}";
		}
		#endregion

		#region Queries

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Figure out where the breakpoint to our right is. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="yScanLine">	Where the scan line resides currently. </param>
		///
		/// <returns>	The X coordinate of the right breakpoint. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal double RightBreak(double yScanLine)
		{
			// If we're the leaf furthest to the right...
			if (RightAdjacentLeaf == null)
			{
				// There is no break to the right - return "infinity"

				return double.MaxValue;
			}

			// Calculate where we intersect the parabola to our right
			return Geometry.ParabolicCut(RightAdjacentLeaf.Poly.VoronoiPoint, Poly.VoronoiPoint, yScanLine);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Figure out where the breakpoint to our left is. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="yScanLine">	Where the scan line resides currently. </param>
		///
		/// <returns>	The X coordinate of the left breakpoint. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal double LeftBreak(double yScanLine)
		{
			// If we're the leaf furthest to the left...
			if (LeftAdjacentLeaf == null)
			{
				// There is no break point to our left - return "minus infinity"

				return double.MinValue;
			}
			// Calculate where we intersect the parabola to our left
			return Geometry.ParabolicCut(Poly.VoronoiPoint, LeftAdjacentLeaf.Poly.VoronoiPoint, yScanLine);
		}
		#endregion

		#region Modification

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Remove the circle event which snuffs this node's parabola from the event queue. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="evq">	Event queue. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void DeleteAssociatedCircleEvent(EventQueue evq)
		{
			// If we've got a circle event
			if (_cevt != null)
			{
				// Delete it
				evq.Delete(_cevt);
				if (_cevt.LinkedListNode != null)
				{
					evq.CircleEvents.Remove(_cevt.LinkedListNode);
				}
				_cevt = null;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Set the circle event which snuff's this node's parabola. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="cevt">	The cevt. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void SetCircleEvent(CircleEvent cevt)
		{
			_cevt = cevt;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Locate the inner node which represents the edge on a selected side. </summary>
		///
		/// <remarks>	
		/// We have to walk up the tree till we find an inner node whose polygon is identical to the
		/// polygon being created by our left sibling. 
		/// 
		/// Darrellp, 2/18/2011. 
		/// </remarks>
		///
		/// <param name="fLeftSibling">	Which side to locate. </param>
		///
		/// <returns>	The inner node for the breakpoint. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal InternalNode InnFindSiblingEdge(bool fLeftSibling)
		{
			// Initialize
			var innCur = NdParent;
			var polyOfSibling = fLeftSibling ? LeftAdjacentLeaf.Poly : RightAdjacentLeaf.Poly;

			// While we haven't found an ancestor node with the same poly as our sibling
			while ((fLeftSibling ? innCur.PolyLeft : innCur.PolyRight) != polyOfSibling)
			{
				// Move up the tree
				innCur = innCur.NdParent;
			}

			// Return the ancestor node we located
			return innCur;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	I'm being removed so link my sibling nodes together as siblings. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void LinkSiblingsTogether()
		{
			// If we've got a left sibling
			if (LeftAdjacentLeaf != null)
			{
				// Make my right sibling it's right sibling
				LeftAdjacentLeaf.RightAdjacentLeaf = RightAdjacentLeaf;
			}

			// If we've got a right sibling
			if (RightAdjacentLeaf != null)
			{
				// Make my left sibling it's left sibling
				RightAdjacentLeaf.LeftAdjacentLeaf = LeftAdjacentLeaf;
			}
		}
		#endregion
	}
}