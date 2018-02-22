using System;
using System.Collections.Generic;
using System.Linq;
using MeshNav;
using static MeshNav.Geometry2D;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Fortune polygon. </summary>
	///
	/// <remarks>
	/// <para>I should point out that classic winged edge polygons don't keep a list of edges on each
	/// polygon.  They are enumerable through the Edges property.  Only a single arbitrary "starting"
	/// edge is kept in the polygon structure.  In general, that is the way winged edges work.  In
	/// the fortune case, we kind of come at the edges in an almost random manner so we keep them in
	/// a list.  The Edges enumeration should still work, though it's slower and unnecessary since
	/// you can just retrieve the list through FortunePoly.FortuneEdges.  We could end up with lower
	/// performance and less space by nulling out the array in the fortune polygons at the end of
	/// processing and relying on the Edges enumeration but we'd still need to keep the arrays around
	/// while we're actually creating the structure so I don't know how much real space we'd
	/// save.</para>
	/// 
	/// Darrellp, 2/18/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public class FortunePoly : WePolygon
	{
		#region Properties

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets a list of edges in Clockwise order. </summary>
		/// <remarks>	
		/// Sadly, it was after creating and using this variable in numerous places that I found out that
		/// the "standard" order for keeping edges is in CCW order.  The enumerator at WePolygon was made
		/// with this convention in mind and thus returns the edges in CCW order so we have the confusion
		/// that one edge enumerator returns edges in CW order and one in CCW order.  Given that they're
		/// both used so heavily it would be tough to remove or modify either one.  I've made this one
		/// internal to avoid confusion externally. 
		/// </remarks>
		///
		/// <value>	The edges in Clockwise order. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal List<FortuneEdge> FortuneEdges { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Adds an edge to the Fortune polygon. </summary>
		///
		/// <remarks>	Darrellp, 2/22/2011. </remarks>
		///
		/// <param name="edge">	The edge to be added. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void AddEdge(FortuneEdge edge)
		{
			FortuneEdges.Add(edge);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the number of vertices. </summary>
		///
		/// <value>	The number of vertices. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public int VertexCount => FortuneEdges.Count;

	    ////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Indicates that this is the singleton polygon at infinity </summary>
		///
		/// <value>	true if at it's the polygon at infinity. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool FAtInfinity { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Tells whether we detected a zero length edge during the fortune processing </summary>
		///
		/// <value>	true if zero length edge is present </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal bool FZeroLengthEdge { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	The original point which caused this voronoi cell to exist. </summary>
		///
		/// <value>	The point in the original set of data points. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public Vector VoronoiPoint { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	A generic index to identify this polygon for debugging purposes.. </summary>
		///
		/// <value>	The index. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal int Index { get; set; }

		#endregion

		#region Queries

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Clip this voronoi cell with a passed in bounding box. 
		/// </summary>
		///
		/// <remarks>	
		/// <para>I'm trying to handle all the exceptional cases.  There is one incredibly exceptional
		/// case which I'm ignoring.  That is the case where there are two vertices at infinity which are
		/// at such nearly opposite directions without being completely collinear that we can't push
		/// their points at infinity out far enough to encompass the rest of the box within the range of
		/// a double.  If this is important to you, then see the comments below, but it's hard to imagine
		/// it ever arising.</para>
		/// 
		/// <para>Editorial comment - The annoying thing about all of this is that, like in so much of
		/// computational geometry, the rarer and less significant the exceptional cases are, the more
		/// difficult they are to handle.  It's both a blessing and a curse - it means that the normal
		/// cases are generally faster, but it also makes it difficult to get excited about slogging
		/// through the tedious details of situations that will probably never arise in practice.  Still,
		/// in order to keep our noses clean, we press on regardless.</para>
		/// 
		/// Darrellp, 2/26/2011. 
		/// </remarks>
		///
		/// <param name="ptUL">	The upper left point of the box. </param>
		/// <param name="ptLR">	The lower right point of the box. </param>
		///
		/// <returns>	An enumerable of real points representing the voronoi cell clipped to the box. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<Vector> BoxVertices(Vector ptUL, Vector ptLR)
		{
			// If no edges, then it's just the entire box
			if (!Edges.Any())
			{
				foreach (var pt in BoxPoints(ptUL, ptLR))
				{
					yield return pt;
				}
				yield break;
			}

		    var ptsBox = BoxPoints(ptUL, ptLR);

			var fFound = FCheckEasy(out var ptsToBeClipped);
			if (!fFound)
			{
				fFound = FCheckParallelLines(ptsBox, out ptsToBeClipped);
			}
			if (!fFound)
			{
				fFound = FCheckDoublyInfinite(ptsBox, out ptsToBeClipped);
			}
			if (!fFound)
			{
				ptsToBeClipped = RealVertices(CalcRayLength(ptsBox));
			}
			foreach (var pt in ConvexPolyIntersection.FindIntersection(ptsToBeClipped, ptsBox))
			{
				yield return pt;
			}
		}

		private bool FCheckParallelLines(List<Vector> ptsBox, out IEnumerable<Vector> ptsToBeClipped)
		{
			// Do the required initialization of our out parameter
			ptsToBeClipped = null;

			// See if our cell is made up of two parallel lines.
			// This is the only case where we will have exactly two lines at
			// infinity - one connecting each end of the parallel lines.
			// We will have exactly six edges - it will look like the following:
			//
			//      Inf vtx-------------finite vtx 0 -----------------Inf vtx
			//      |                                                       |
			//      |<-Edge at infinity                   Edge at infinity->|
			//      |                                                       |
			//      Inf vtx-------------finite vtx 1 ------------------Inf vtx
			//
			// So that's a total of six edges and two of them at infinity.
			if (Edges.Count(e => e.FAtInfinity) == 2)
			{
				// Retrieve the two finite points
				var ptsFinite = Vertices.Where(v => !v.FAtInfinity).Select(v => v.Pt).ToArray();

				// Find out the max dist from any finite point to any finite point on the box and double it for good measure
				var maxDist0 = Math.Sqrt(ptsBox.Select(pt => DistanceSq(pt, ptsFinite[0])).Max());
				var maxDist1 = Math.Sqrt(ptsBox.Select(pt => DistanceSq(pt, ptsFinite[1])).Max());
				var maxDist = 2.0 * Math.Max(maxDist0, maxDist1);

				// Use that as a ray length to get real vertices which will later be clipped to the box
				ptsToBeClipped = RealVertices(maxDist);
				return true;
			}
			return false;
		}

		private bool FCheckDoublyInfinite(List<Vector> ptsBox, out IEnumerable<Vector> ptsToBeClipped)
		{
			ptsToBeClipped = null;

			// This case will always have exactly three vertices - one finite and two infinite
			if (Vertices.Count() != 3)
			{
				return false;
			}

			// Get the finite vertex
			WeVertex vtx = null;
			var vtxs = Vertices.ToArray();
			var ptTailDir = new Vector(0, 0);
			var ptHeadDir = new Vector(0, 0);
			for (var ivtx = 0; ivtx < 3; ivtx++)
			{
				if (vtxs[ivtx].FAtInfinity)
				{
					continue;
				}
				ptTailDir = vtxs[(ivtx + 1) % 3].Pt;
				ptHeadDir = vtxs[(ivtx + 2) % 3].Pt;
				vtx = vtxs[ivtx];
				break;
			}

			// If it's only got two edges emanating from it (a doubly infinite line)
			// ReSharper disable once PossibleNullReferenceException
			if (vtx.Edges.Count() == 2)
			{
				// TODO: We should probably do this manually rather than relying on ConvexPolyIntersection

				// Figure out a satisfactory distance for our ray length
				var maxDist = 2.0 * Math.Sqrt(ptsBox.Select(pt => DistanceSq(pt, vtx.Pt)).Max());
				var ptTail = vtx.Pt + ptTailDir*maxDist;
				var ptHead = vtx.Pt + ptHeadDir*maxDist;

				// For every point in the box
				//
				// We have to include the points to the left of the line in our rectangle we want clipped
				// So find the max distance and extend a rect that length to the side of our line
				// segment.  We double the offset just to be safe.

				var leftDistances = ptsBox.
					// Get the distance to our line
					Select(p => PtToLineDistance(p, ptHead, ptTail)).
					// Keep only the ones to the left of the line
					Where(d => d > 0);

				// If there were no points to the left
			    var leftDistancesArray = leftDistances as double[] ?? leftDistances.ToArray();
			    if (!leftDistancesArray.Any())
				{
					// return an empty array
					ptsToBeClipped = new List<Vector>();
					return true;
				}
				
				// Our rect width will be twice the largest of the left distances
				var maxLineDist = 2 * leftDistancesArray.Max();

				// Return the rectangle formed from our line segment extended out by maxLineDist
				var vcOffset = (ptTail - ptHead).Normalize().Flip90Ccw()*maxLineDist;
				//var vcOffset = (ptHead - ptTail).Normalize().Flip90Ccw() * maxLineDist;
				ptsToBeClipped = new List<Vector>
				                 	{
				                 		ptHead,
				                 		ptTail,
				                 		ptTail + vcOffset,
				                 		ptHead + vcOffset
				                 	};
				return true;
			}
			return false;
		}

		private bool FCheckEasy(out IEnumerable<Vector> ptsToBeClipped)
		{
			ptsToBeClipped = null;
			if (!Edges.Any(e => e.VtxStart.FAtInfinity || e.VtxEnd.FAtInfinity))
			{
				ptsToBeClipped = Vertices.Select(v => v.Pt);
				return true;
			}
			return false;
		}


		private double CalcRayLength(List<Vector> ptsBox)
		{
			// Initialize
			var oes = OrientedEdges.ToArray();
			var ioeOutgoing = 0;

			// For each oriented edge
			for (int i = 0; i < oes.Length; i++)
			{
				// If it's the outgoing infinite edge
				if (oes[i].EndVtx.FAtInfinity)
				{
					// Set ioeOutgoing to it's index
					ioeOutgoing = i;
					break;
				}
			}

			// Get the outgoing and incoming infinite edges.
			//
			// The incoming edge is always two further away from the outgoing,
			// separated by the edge at infinity.
			var oeOut = oes[ioeOutgoing];
			var oeIn = oes[(ioeOutgoing + 2)%oes.Length];

			// Make an initial guess for a good ray length
			double length = CalcInitialGuess(oeIn, oeOut, ptsBox);

			// While the length is still not satisfactory
			while (!Satisfactory(length, oeIn, oeOut, ptsBox) && length < double.MaxValue / 2.0)
			{
				// Double it
				length *= 2;
			}

			// Return the final length
			return length;
		}

		private static bool Satisfactory(double length, OrientedEdge oeIn, OrientedEdge oeOut, IEnumerable<Vector> ptsBox)
		{
			// The length is satisfactory if all the points in the box are on the same side of it
			var ptRealOut = oeOut.EndVtx.ConvertToReal(oeOut.StartPt, length);
			var ptRealIn = oeIn.StartVtx.ConvertToReal(oeIn.EndPt, length);
			return ptsBox.All(pt => FLeft(ptRealOut, ptRealIn, pt));
		}

		private static double CalcInitialGuess(OrientedEdge oeIn, OrientedEdge oeOut, List<Vector> ptsBox)
		{
			// Find out the max dist from any finite point to any point on the box and double it for good measure
			var maxDist0 = Math.Sqrt(ptsBox.Select(pt => DistanceSq(pt, oeIn.StartPt)).Max());
			var maxDist1 = Math.Sqrt(ptsBox.Select(pt => DistanceSq(pt, oeOut.StartPt)).Max());
			return 2.0 * Math.Max(maxDist0, maxDist1);

		}

		private static List<Vector> BoxPoints(Vector ptUL, Vector ptLR)
		{
			return new List<Vector>
			       	{
			       		new Vector(ptUL.X, ptLR.Y),
			       		new Vector(ptLR.X, ptLR.Y),
			       		new Vector(ptLR.X, ptUL.Y),
			       		new Vector(ptUL.X, ptUL.Y)
			       	};
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Identical to BoxVertices if there are any infinite polygons/vertices involved.  In the case
		/// of a purely finite polygon, we return it unaltered. 
		/// </summary>
		///
		/// <remarks>	
		/// This overload of RealVertices ensures that all vertices are finite in a foolproof way.  The
		/// overload of RealVertices that takes a double as a raylength relies on the caller guessing as
		/// to a sufficient ray length to ensure that our rays extend outside of whatever area they're
		/// interested in.  That's usually not possible to do in a foolproof way.  Also, it has problems
		/// with doubly infinite lines when the return value is interpreted as a polygon.  This routine
		/// doesn't necessarily clip to the box in question, but does guarantee that the finite polygon
		/// returned will properly cover it's assigned area in the box passed in.  On interior cells, it
		/// avoids the overhead of a clipping operation which is liable to happen outside of this call
		/// anyway.  This is the safest way of covering a box without necessarily clipping to it.
		/// 
		/// Darrellp, 2/28/2011. 
		/// </remarks>
		///
		/// <param name="ptUL">	The upper left point of the box. </param>
		/// <param name="ptLR">	The lower right point of the box. </param>
		///
		/// <returns>	An enumerable of real points representing the polygon. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<Vector> RealVertices(Vector ptUL, Vector ptLR)
		{
			if (!Edges.Any(e => e.VtxStart.FAtInfinity || e.VtxEnd.FAtInfinity))
			{
				foreach (var pt in Vertices.Select(v => v.Pt))
				{
					yield return pt;
				}
			}
			else
			{
				foreach (var pt in BoxVertices(ptUL, ptLR))
				{
					yield return pt;
				}
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Converts vertices to real vertices. </summary>
		///
		/// <remarks>	
		/// This routine does nothing for polygons with no infinite vertices.  If there are no doubly
		/// infinite lines, it uses raylength to extend the rays.  If there are doubly infinite lines it
		/// uses the machinery above to produce a region.  No clipping is done but it's guaranteed is
		/// that the resultant polygon is finite and covers the polygon's portion which intersects the
		/// passed in box (assuming that rayLength is long enough).  This routine is fairly fast, works
		/// with doubly infinite lines but could fail for infinite regions in which rayLength is of
		/// insufficient size.  If you feel like raylength will handle all non-doubly infinite lines but
		/// you still have to deal with the doubly infinite case, this is a good choice - especially if
		/// perfomance is an issue.
		/// 
		/// Darrellp, 3/1/2011. 
		/// </remarks>
		///
		/// <param name="rayLength">	Length of the ray. </param>
		/// <param name="ptUL">			The upper left point of the box. </param>
		/// <param name="ptLR">			The lower right point of the box. </param>
		///
		/// <returns>	An enumerable of real points representing the polygon. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		// TODO: share code between this and BoxVertices
		public IEnumerable<Vector> RealVertices(double rayLength, Vector ptUL, Vector ptLR)
		{
			// If no edges, then it's just the entire box
			if (!Edges.Any())
			{
				foreach (var pt in BoxPoints(ptUL, ptLR))
				{
					yield return pt;
				}
				yield break;
			}

		    var ptsBox = BoxPoints(ptUL, ptLR);

			var fFound = FCheckEasy(out var points);
			if (!fFound)
			{
				fFound = FCheckParallelLines(ptsBox, out points);
			}
			if (!fFound)
			{
				fFound = FCheckDoublyInfinite(ptsBox, out points);
			}
			if (!fFound)
			{
				points = RealVertices(rayLength);
			}
			foreach (var pt in points)
			{
				yield return pt;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Return real vertices.  All vertices at infinity will be converted based on the passed ray
		/// length. 
		/// </summary>
		///
		/// <remarks>	
		/// There are a bewildering number of overloads and variations on RealVertices.  They are all
		/// meant to convert infinite vertices to finite ones and return a list of those finite vertices.
		/// This one is the simplest and fastest but also the most unreliable.  It takes a rayLength and
		/// simply extends each infinite line out to the length passed in.  Depending on what is desired,
		/// this may be sufficient, however if the ray length is not chosen properly the rays may not
		/// extend far enough.  Also, if the points returned are looked at as a polygon to be filled,
		/// doubly infinite lines will return empty polygons since they will just exted out in opposite
		/// directions making a line segmennts.  In some situations, doubly infinite lines are not a
		/// concern and often in those cases, this is the overload of choice.  Otherwise you'll have to
		/// continue on to one of the other overloads.
		/// 
		/// Darrellp, 2/23/2011. 
		/// </remarks>
		///
		/// <param name="rayLength">	Length of the ray. </param>
		///
		/// <returns>	An enumerable of real points representing the polygon. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IEnumerable<Vector> RealVertices(double rayLength)
		{
			// For every edge in the polygon
			foreach (var oe in OrientedEdges)
			{
				// Skip if it's the edge at infinity - we'll pick it up on one of the infinite edges
				if (oe.Edge.FAtInfinity)
				{
					continue;
				}

				// Get start and end vertices
				var vtxStart = oe.StartVtx;
				var vtxEnd = oe.EndVtx;

				// If the start vtx is at infinity
				if (vtxStart.FAtInfinity)
				{
					// Then return it to count for the edge at infinity
					yield return vtxStart.ConvertToReal(vtxEnd.Pt, rayLength);
				}
				
				// If the end vertex is at infinity
				if (vtxEnd.FAtInfinity)
				{
					// convert it to a real point and return it
					yield return vtxEnd.ConvertToReal(vtxStart.Pt, rayLength);
				}
				else
				{
					// return it normally
					yield return vtxEnd.Pt;
				}
			}
		}
		#endregion

		#region Constructor

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Constructor. </summary>
		///
		/// <remarks>	Darrellp, 2/18/2011. </remarks>
		///
		/// <param name="pt">		The point which creates this polygon. </param>
		/// <param name="index">	The index to identiry this polygon. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal FortunePoly(Vector pt, int index)
		{
			FZeroLengthEdge = false;
			FAtInfinity = false;
			Index = index;
			VoronoiPoint = pt;
			FortuneEdges = new List<FortuneEdge>();
		}
		#endregion

		#region Edge operations

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Sort the edges in Clockwise order. </summary>
		///
		/// <remarks>	
		/// We do this partially by knowing that all the polygons in a Voronoi diagram are convex.  That
		/// means we can sort edges by measuring their angle around the generator for the polygon.  We
		/// have to pick the point to measure this angle carefully which is what
		/// WEEdge.PolyOrderingTestPoint() does.  We also have to make a special case for the rare doubly
		/// infinite lines (such as that created with only two generators).
		/// 
		/// Darrellp, 2/18/2011. 
		/// </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void SortEdges()
		{
			// If there are only two edges
			if (FortuneEdges.Count == 2)
			{
				// If they are split
				if (FortuneEdges[0].FSplit)
				{
					// If they need reordering
					//
					// I don't think this is really necessary.  It makes sense to order more than 2
					// edges because they really have an "order" which will be maintained no matter which
					// polygon they're ordered by (though the starting edge may change).  With two oppositely
					// directed rays as in this case, there is no such "order" independent of the polygon that
					// does the ordering.  I'm leaving it in but I don't think it's necessary.
					// TODO: Check on this!
					if (ICcw(
						FortuneEdges[0].PolyOrderingTestPoint,
						FortuneEdges[1].PolyOrderingTestPoint,
						VoronoiPoint) < 0)
					{
						// Reorder them
						var edgeT = FortuneEdges[0];
						FortuneEdges[0] = FortuneEdges[1];
						FortuneEdges[1] = edgeT;
					}
				}
				else
				{
					// I think this represents an infinite polygon with only two edges.

					// If not ordered around the single base point properly
					if (ICcw(FortuneEdges[0].VtxStart.Pt,
					                  FortuneEdges[0].PolyOrderingTestPoint,
					                  FortuneEdges[1].PolyOrderingTestPoint) > 0)
					{
						// Swap the edges
						var edgeT = FortuneEdges[0];
						FortuneEdges[0] = FortuneEdges[1];
						FortuneEdges[1] = edgeT;
					}
				}
			}
			else if (FortuneEdges.Count == 4 && FortuneEdges[0].FSplit)
			{
				// I think the nontransitivity of the "order" here will cause the CLR Sort()
				// to screw up our ordering in this case so we handle it specially...
				if (ICcw(FortuneEdges[0].VtxStart.Pt,
					FortuneEdges[0].PolyOrderingTestPoint,
					FortuneEdges[1].PolyOrderingTestPoint) > 0)
				{
					var edgeT = FortuneEdges[0];
					FortuneEdges[0] = FortuneEdges[1];
					FortuneEdges[1] = edgeT;
				}
				if (ICcw(FortuneEdges[2].VtxStart.Pt,
					FortuneEdges[2].PolyOrderingTestPoint,
					FortuneEdges[3].PolyOrderingTestPoint) > 0)
				{
					var edgeT = FortuneEdges[2];
					FortuneEdges[2] = FortuneEdges[3];
					FortuneEdges[3] = edgeT;
				}
			}
			else {
				// More than 3 vertices just get a standard CLR sort
				// TODO: Is nontransitivity screwing up here too and we're just not noticing?
				FortuneEdges.Sort();
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Remove an edge. </summary>
		///
		/// <remarks>	This really only makes much sense for zero length edges. </remarks>
		///
		/// <param name="edge">	Edge to remove. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void DetachEdge(FortuneEdge edge)
		{
			// Remove the zero length edge
			//
			// We do this by removing it's end vertex, reassigning the proper
			// vertex in each of the edges which formerly connected to that vertex and splicing those edges into the
			// the proper spot for the edge list of our start vertex and finally removing it from the edge list of
			// both polygons which it adjoins.
			edge.ReassignVertexEdges();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Sort out zero length edge issues. </summary>
		///
		/// <remarks>	
		/// Zero length edges are a pain and have to be dealt with specially since they don't sort
		/// properly using normal geometrical position nor can "sidedness" be determined solely from
		/// their geometry (a zero length line has no "sides").  Instead, we have to look at the non-zero
		/// length edges around them and determine this information by extrapolating from those edges
		/// topological connection to this edge. 
		/// </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void HandleZeroLengthEdges()
		{
			// If no zero length edges
			if (!FZeroLengthEdge)
			{
				// We're outta here
				return;
			}

			// For every edge in the polygon
			for (var i = 0; i < VertexCount; i++)
			{
				// Retrieve the edge
				var edgeCheck = FortuneEdges[i];

				// If it's zero length
				if (edgeCheck.FZeroLength())
				{
					// Remove the edge from both this polygon and the polygon "across" the zero length edge
					DetachEdge(edgeCheck);
					FortuneEdges.Remove(edgeCheck);
					edgeCheck.OtherPoly(this).FortuneEdges.Remove(edgeCheck);

					// We have to back up one because we deleted edge i
					i--;
				}
			}
		}
		#endregion
	}
}