using System;
using System.Collections.Generic;
using System.Linq;
using MeshNav;
using static MeshNav.Geometry2D;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Static class to hold the convex poly intersection routine. </summary>
	/// <remarks>	Darrellp, 2/23/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////
	public static class ConvexPolyIntersection
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Finds the intersection of two convex polygons. </summary>
		/// <remarks>
		///     <para>
		///         No check is made for convexity.  The enumerables must yield the points in counterclockwise
		///         order.
		///     </para>
		///     <para>
		///         This code works by looking for intersections between the two polygons.  If there is no intersection
		///         then no points will be returned even if one is wholly contains within the other.  Putting a check in for
		///         this case is often unnecessary and so we leave it out here and plan on incorporating it in a separate
		///         method at a later date.
		///     </para>
		///     <para>This is based on the code in "Computational Geometry in C" by Joseph O'Rourke.</para>
		///     Darrellp, 2/23/2011.
		/// </remarks>
		/// <param name="poly1Enum">	The polygon 1 enum. </param>
		/// <param name="poly2Enum">	The polygon 2 enum. </param>
		/// <returns>	An enumeration of the points in the intersection. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public static IEnumerable<Vector> FindIntersection(IEnumerable<Vector> poly1Enum, IEnumerable<Vector> poly2Enum)
		{
			// Put the two polygons into arrays
			var polyA = poly1Enum.ToArray();
			var polyB = poly2Enum.ToArray();

			// If poly1 is empty, return poly2
			if (!polyA.Any())
			{
				foreach (var poly in polyB)
				{
					yield return poly;
				}

				yield break;
			}

			// If poly2 is empty, return poly1
			if (!polyB.Any())
			{
				foreach (var poly in polyA)
				{
					yield return poly;
				}

				yield break;
			}

			// Initialize

			// Index of the heads that chase each other around the polygon
			var aCur = 0;
			var bCur = 0;
			var origin = new Vector(0, 0);

			// Tells whether A or B is currently on the "inside".
			var inflag = InflagVals.Unknown;

			// Total number of times we've advanced each of the heads
			var cAdvancesA = 0;
			var cAdvancesB = 0;

			// True after we've found the first point
			var fFoundFirstPoint = false;

			// Counts of vertices in the two polygons
			var cPolyAVertices = polyA.Length;
			var cPolyBVertices = polyB.Length;

			// Last point we output so we don't repeat points
			var ptPrevOutput = new Vector();
			var ptFirstOutput = new Vector();

			// Step through the edges
			do
			{
				// Compute key variables

				// Tail of our current sides
				var aPrev = (aCur + cPolyAVertices - 1) % cPolyAVertices;
				var bPrev = (bCur + cPolyBVertices - 1) % cPolyBVertices;

				// Direction of each side
				var vecA = polyA[aCur] - polyA[aPrev];
				var vecB = polyB[bCur] - polyB[bPrev];

				// cross > 0 means a counterclockwise turn from vector A to vector B
				var cross = Math.Sign(SignedArea(origin, vecA, vecB));

				// Whether the left half plane of each vector contains the head of the other vector
				var bHalfPlaneContainsA = Math.Sign(SignedArea(polyB[bPrev], polyB[bCur], polyA[aCur]));
				var aHalfPlaneContainsB = Math.Sign(SignedArea(polyA[aPrev], polyA[aCur], polyB[bCur]));

				// if A & B intersect
				(var code, var ptCrossing) = SegSegInt(polyA[aPrev], polyA[aCur], polyB[bPrev], polyB[bCur]);
				if (code == CrossingType.Normal || code == CrossingType.Vertex)
				{
					// If this is the first intersection we've seen
					if (inflag == InflagVals.Unknown && !fFoundFirstPoint)
					{
						// Set First Point Found
						fFoundFirstPoint = true;
						ptFirstOutput = ptCrossing;
						cAdvancesA = cAdvancesB = 0;
					}
					// Else if we've looped back on ourselves
					else if (ptCrossing.Equals(ptFirstOutput))
					{
						yield break;
					}

					// update the inflag
					inflag = InOut(inflag, bHalfPlaneContainsA, aHalfPlaneContainsB);
					if (!ptCrossing.Equals(ptPrevOutput))
					{
						yield return ptCrossing;
						ptPrevOutput = ptCrossing;
					}
				}

				// If A and B overlap and are oppositely oriented
				//
				// This means that one edge of the polys meets the edge of the other with the polygons lying on
				// opposite sides so that this overlap is the entirety of the overlap for the polygons.  We've
				// already returned the points so nothing to do here but quit out.
				if (code == CrossingType.Edge && Dot(vecA, vecB) < 0)
				{
					yield break;
				}

				// else if A and B are parallel and separated
				//
				// The union of the two polygons is empty
				if (cross == 0 && bHalfPlaneContainsA < 0 && aHalfPlaneContainsB < 0)
				{
					// Handle it
					yield break;
				}

				// else if A and B are collinear
				if (cross == 0 && bHalfPlaneContainsA == 0 && aHalfPlaneContainsB == 0)
				{
					// Advance, but don't output point
					if (inflag == InflagVals.AInterior)
					{
						bCur = Advance(bCur, ref cAdvancesB, cPolyBVertices);
					}
					else
					{
						aCur = Advance(aCur, ref cAdvancesA, cPolyAVertices);
					}
				}
				// else if A to B is a CCW turn
				else if (cross >= 0)
				{
					// Is B's head to A's left?
					if (aHalfPlaneContainsB > 0)
					{
						// Is A interior to B?
						if (inflag == InflagVals.AInterior && !polyA[aCur].Equals(ptPrevOutput) && !polyA[aCur].Equals(ptFirstOutput))
						{
							// Yeild A's head
							yield return polyA[aCur];
							ptPrevOutput = polyA[aCur];
						}

						// Advance A
						aCur = Advance(aCur, ref cAdvancesA, cPolyAVertices);
					}
					else
					{
						// Is B interior to A?
						if (inflag == InflagVals.BInterior && !polyB[bCur].Equals(ptPrevOutput) && !polyB[bCur].Equals(ptFirstOutput))
						{
							// Yeild B's head
							yield return polyB[bCur];
							ptPrevOutput = polyB[bCur];
						}

						// Advance B
						bCur = Advance(bCur, ref cAdvancesB, cPolyBVertices);
					}
				}
				else
				{
					// Is A's head to B's right?
					if (bHalfPlaneContainsA < 0)
					{
						// Is A interior to B?
						if (inflag == InflagVals.AInterior && !polyA[aCur].Equals(ptPrevOutput) && !polyA[aCur].Equals(ptFirstOutput))
						{
							// Yeild A's head
							yield return polyA[aCur];
							ptPrevOutput = polyA[aCur];
						}

						// Advance A
						aCur = Advance(aCur, ref cAdvancesA, cPolyAVertices);
					}
					else
					{
						// Is B interior to A?
						if (inflag == InflagVals.BInterior && !polyB[bCur].Equals(ptPrevOutput) && !polyB[bCur].Equals(ptFirstOutput))
						{
							// Yeild B's head
							yield return polyB[bCur];
							ptPrevOutput = polyB[bCur];
						}

						// Advance B
						bCur = Advance(bCur, ref cAdvancesB, cPolyBVertices);
					}
				}
			}
			// both indices have cycled or one has cycled twice
			while (
				(cAdvancesA < cPolyAVertices || cAdvancesB < cPolyBVertices) &&
				cAdvancesA < 2 * cPolyAVertices && cAdvancesB < 2 * cPolyBVertices);

			// If Inflags is unknown then we never intersected and may have one poly wholly contained in the other.
			if (inflag == InflagVals.Unknown)
			{
				// If a point of A is in B
				if (PointInConvexPoly(polyA[0], polyB))
				{
					// Yield all of A
					foreach (var pt in polyA)
					{
						yield return pt;
					}
				}
				// else if a point of B is in A
				else if (PointInConvexPoly(polyB[0], polyA))
				{
					// Yield all of B
					foreach (var pt in polyB)
					{
						yield return pt;
					}
				}
			}
		}

		private static int Advance(int iHead, ref int cAdvances, int cPolyVertices)
		{
			cAdvances++;
			return (iHead + 1) % cPolyVertices;
		}

		private static InflagVals InOut(InflagVals inflag, int aHalfPlaneContainsB, int bHalfPlaneContainsA)
		{
			if (aHalfPlaneContainsB > 0)
			{
				return InflagVals.AInterior;
			}

			if (bHalfPlaneContainsA > 0)
			{
				return InflagVals.BInterior;
			}

			return inflag;
		}

		private enum InflagVals
		{
			AInterior,
			BInterior,
			Unknown
		}
	}
}