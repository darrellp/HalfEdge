using System.Collections.Generic;
using System.Linq;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	///     Winged Edge data structure for representing a B-rep or "boundary representation".
	/// </summary>
	/// <remarks>
	///     Intuitively,
	///     a B-rep can be though of as the representation for a map of polygonal countries.  This is
	///     precisely what the voronoi diagram returns.  Winged Edge is a bit redundant in it's
	///     representation but provides a very flexible structure for working with the underlying B-rep.
	///     WingedEdge centers on the edges and represents each edge by the polygons on each side of it
	///     and the next edge in each of those polygons.  Geometrically, the information retained with
	///     each edge looks like:
	///     \      /
	///     Other Edges		 \____/    Other Edges
	///     /    \
	///     /      \
	///     The "WingedEdge" name comes from the resemblance of this diagram to a butterfly.
	///     The version here is not much more than a shell.  It has some validation routines and some
	///     navigation routines, but it's pretty much up to the user to set up the structure via adding
	///     Polygons, vertices and edges to the structure and ensuring that their fields are set up
	///     correctly.
	///     Darrellp, 2/18/2011.
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////
	public class WingedEdge<TPoly, TEdge, TVtx>
		where TPoly : WePolygon
		where TEdge : WeEdge
		where TVtx : WeVertex
	{
		#region Constructor
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Default constructor. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public WingedEdge()
		{
			LstVertices = new List<TVtx>();
			LstEdges = new List<TEdge>();
			LstPolygons = new List<TPoly>();
		}
		#endregion

		#region Properties
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the list of contained polygons. </summary>
		/// <value>	The list of contained polygons. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public List<TPoly> LstPolygons { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the list of contained edges. </summary>
		/// <value>	The list of contained edges. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public List<TEdge> LstEdges { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the list of contained vertices. </summary>
		/// <value>	The list of contained vertices. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public List<TVtx> LstVertices { get; set; }
		#endregion

		#region Validation
		/// <summary>
		///     Set a breakpoint in this routine to find out when Validate() fails.
		/// </summary>
		/// <returns>Always false</returns>
		private static bool Failure()
		{
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Validates the WingedEdge structure in several ways. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		/// <returns>	true if the winged edge structure is valid. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool Validate()
		{
			// Each edge has two vertices associated with it so we can have at most twice as many
			// vertices as edges...
			if (LstVertices.Count > 2 * LstEdges.Count)
			{
				return Failure();
			}

			// Validate the edges one by one
			if (LstEdges.Any(edge => !edge.Validate()))
			{
				return Failure();
			}

			// Validate the polygons one by one
			if (LstPolygons.Any(poly => !poly.FValidateEdgesInOrder()))
			{
				return Failure();
			}

			return true;
		}
		#endregion

		#region Adding elements
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Adds an edge. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		/// <param name="edge">	The edge to be added. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public void AddEdge(TEdge edge)
		{
			LstEdges.Add(edge);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Adds a polygon. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		/// <param name="polygon">	The polygon to be added. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public void AddPoly(TPoly polygon)
		{
			LstPolygons.Add(polygon);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Adds a vertex. </summary>
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		/// <param name="vertex">	The vertex to be added. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public void AddVertex(TVtx vertex)
		{
			LstVertices.Add(vertex);
		}
		#endregion
	}
}