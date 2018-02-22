
namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Internal node in the tree representing our beachline. </summary>
	///
	/// <remarks>	
	/// Internal nodes represent edges developing in the voronoi diagram being created.  As such,
	/// they have a fortune edge which will ultimately end up in the winged edge representation and
	/// two winged edge polygons which will also end up in the final rep.  In terms of the beachline
	/// tree, leaf nodes represent individual polygons in the voronoi diagram and the edge
	/// corresponding to an internal node is the edge between polygons represented by the rightmost
	/// leaf node in it's left subtree and the leftmost leaf node in it's right subtree.  
	/// 
	/// Keeping in mind the equivalence between the beachline, the tree representing the beachline and the
	/// evolving elements in the voronoi diagram is a big key to understanding this code and the
	/// Fortune algorithm in general.
	/// 
	/// Darrellp, 2/18/2011. 
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	internal class InternalNode : Node
	{
		#region Properties

		// Winged edge that this internal node represents
		internal FortuneEdge Edge { get; set; }

		// Height of left subtree minus height of right for balancing
		internal int DHtLeftMinusRight { get; set; }

		// Winged edge polygon on one side of us
		internal FortunePoly PolyRight { get; set; }

		// Winged edge polygon on the other
		internal FortunePoly PolyLeft { get; set; }

		#endregion

		#region Manipulation
		//!+ TODO: Implement height balanced tree
		// Right now we don't bother trying to keep the beachline tree balanced in any way.
		internal void IncDht()
		{
			DHtLeftMinusRight++;
		}

		internal void DecDht()
		{
			DHtLeftMinusRight--;
		}
		#endregion

		#region Information

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Current edge x coordinate position at the scan line height. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="yScanLine">	The y coordinate scan line. </param>
		///
		/// <returns>	X coordinate where our edge crosses the scan line. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal double CurrentEdgeXPos(double yScanLine)
		{
			return Geometry.ParabolicCut(PolyRight.VoronoiPoint, PolyLeft.VoronoiPoint, yScanLine);
		}
		#endregion

		#region Edge handling

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Sets our edge for this internal node. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="edge">	The edge. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void SetEdge(FortuneEdge edge)
		{
			Edge = edge;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Adds an edge to the fortune polygons we abut. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="edge">	The edge. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void AddEdgeToPolygons(FortuneEdge edge)
		{
			PolyLeft.AddEdge(edge);
			PolyRight.AddEdge(edge);
		}
		#endregion

		#region Constructor
		internal InternalNode(FortunePoly polyLeft, FortunePoly polyRight)
		{
			DHtLeftMinusRight = 0;
			PolyLeft = polyLeft;
			PolyRight = polyRight;
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return $"InternNode: Gens = {PolyLeft.Index}, {PolyRight.Index}";
		}
		#endregion
	}
}