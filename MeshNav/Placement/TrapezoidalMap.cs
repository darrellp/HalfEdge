using System.Diagnostics;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	class TrapezoidalMap
	{
		internal Trapezoid Bbox { get; }

		public TrapezoidalMap()
		{
			Bbox = Trapezoid.Bbox();
		}

		// The halfedge is guaranteed to go left to right so if it's face is CCW then it's above the edge
		// else it's below.
		private (Face below, Face above) GetFaces(HalfEdge edge)
		{
			// TODO: think about using crossings over ray hanging down from midpoint of edge
			// I think it would be better performance but probably not enough to worry about

			// Determine which face is above our edge and which is below.  This depends on the orientation
			// of the faces.
			var testFace = edge.Face;
			var otherFace = edge.OppositeFace;
			var reverseSense = false;
			Face aboveFace, belowFace;

			if (testFace == null || testFace.IsBoundary || testFace.IsRayed)
			{
				testFace = edge.OppositeFace;
				otherFace = edge.Face;
				reverseSense = true;
			}

			if (testFace.IsCcw ^ reverseSense)
			{
				aboveFace = testFace.IsBoundary ? null : testFace;
				belowFace = otherFace.IsBoundary ? null : otherFace;
			}
			else
			{
				aboveFace = otherFace.IsBoundary ? null : otherFace;
				belowFace = testFace.IsBoundary ? null : testFace;
			}
			return (belowFace, aboveFace);
		}

		public PlacementNode UpdateMiddleTrapezoid(Trapezoid oldTrap, HalfEdge edge, ref Trapezoid lNeighborTop, ref Trapezoid lNeighborBottom)
		{
			(var belowFace, var aboveFace) = GetFaces(edge);

			Trapezoid above, below;
			TrapNode aboveNode, belowNode;

			Debug.Assert(oldTrap.LeftTop == null ^ oldTrap.LeftBottom == null);

			if (oldTrap.LeftTop == null)
			{
				// Left side originates above so merge bottom trapezoids
				below = lNeighborBottom;
				belowNode = below.Node;
				below.RightVtx = oldTrap.RightVtx;
			}
			else
			{
				below = new Trapezoid
				{
					RightVtx = oldTrap.RightVtx,
					LeftVtx = oldTrap.LeftVtx,
					LeftTop = lNeighborBottom,
					BottomEdge = oldTrap.BottomEdge,
					TopEdge = edge,
					ContainingFace = belowFace
				};
				belowNode = new TrapNode(below);
				lNeighborBottom.RightTop = below;
			}

			if (oldTrap.LeftBottom == null)
			{
				above = lNeighborTop;
				aboveNode = above.Node;
				above.RightVtx = oldTrap.RightVtx;
			}
			else
			{
				above = new Trapezoid
				{
					RightVtx = oldTrap.RightVtx,
					LeftVtx = oldTrap.LeftVtx,
					BottomEdge = edge,
					TopEdge = oldTrap.TopEdge,
					ContainingFace = aboveFace
				};
				aboveNode = new TrapNode(above);
				lNeighborTop.RightBottom = above;
			}

			lNeighborTop = above;
			lNeighborBottom = below;
			return new YNode(edge.InitVertex.Position, edge.NextVertex.Position, aboveNode, belowNode);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		///  <summary>	Updates the Trapezoid which an endpoint of the edge is located in. </summary>
		/// 
		///  <remarks>	Note - we presume the caller is modifying trapezoids intersected by the side from
		///  			left to right.  The lNeighborTop and lNeighborBottom are the top and bottom
		///  			trapezoids created by the edge splitting in the trapezoid to our left.  This is
		///  			so we can merge one of our upper or lower traps with the one on the left. Generally
		/// 				we can't merge right traps into left ones because the right trapezoids haven't been
		/// 				created yet.
		///  
		/// 				Darrell Plank, 2/12/2018. </remarks>
		///  
		///  <param name="oldTrap">	The trapNode which contains the left endpoint. </param>
		///  <param name="edge">	   	The edge whose left endpoint is in the trapezoid. </param>
		/// <param name="lNeighborTop"> The left neighbor's top trapezoid </param>
		/// <param name="lNeighborBottom"> The left neighbor's bottom trapezoid </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		public PlacementNode  UpdateEndTrapezoid(Trapezoid oldTrap, HalfEdge edge, ref Trapezoid lNeighborTop, ref Trapezoid lNeighborBottom)
		{
			(var belowFace, var aboveFace) = GetFaces(edge);

			// The edge splits some portion of our trapezoid vertically.  Determine the extent of that
			// region and which vertices bound it.
			var trapLeft = oldTrap.LeftVtx.X;
			var trapRight = oldTrap.RightVtx.X;
			var edgeLeft = edge.InitVertex.X;
			var edgeRight = edge.NextVertex.X;

			// "meets" in this context means the side shares a previously established vertex.
			// "exceeds" means "goes past" which means it splits through 
			// the side of the trap.  If neither of these is true, it ends in the proper
			// interior of the trapezoid.
			
			// We can use equality here since these will be coming from a shared vertex
			// ReSharper disable CompareOfFloatsByEqualityOperator
			var meetsLeft = trapLeft == edgeLeft;
			var meetsRight = trapRight == edgeRight;
			// ReSharper restore CompareOfFloatsByEqualityOperator
			// 
			var exceedsLeft = trapLeft > edgeLeft;
			var exceedsRight = trapRight < edgeRight;
			var interiorLeft = trapLeft < edgeLeft;
			var interiorRight = trapRight > edgeRight;

			var midRightVtx = meetsRight || exceedsRight ? oldTrap.RightVtx : edge.NextVertex;
			var midLeftVtx = meetsLeft || exceedsLeft ? oldTrap.LeftVtx : edge.InitVertex;

			// The newly created traps to be inserted into the map
			Trapezoid above, below, left, right;

			// Create left trapezoid if necessary
			if (interiorLeft)
			{
				left = new Trapezoid
				{
					RightVtx = midLeftVtx,
					LeftVtx = oldTrap.LeftVtx,
					LeftTop = oldTrap.LeftTop,
					LeftBottom = oldTrap.LeftBottom,
					BottomEdge = oldTrap.BottomEdge,
					TopEdge = oldTrap.TopEdge,
					ContainingFace = oldTrap.ContainingFace
				};
				oldTrap.LeftBottom?.LinkTo(oldTrap, left, true);
				oldTrap.LeftTop?.LinkTo(oldTrap, left, true);
			}
			else
			{
				left = null;
			}

			// Create right trapezoid if necessary
			if (interiorRight)
			{
				right = new Trapezoid
				{
					RightVtx = oldTrap.RightVtx,
					LeftVtx = midRightVtx,
					RightBottom = oldTrap.RightBottom,
					LeftBottom = oldTrap.LeftBottom,
					BottomEdge = oldTrap.BottomEdge,
					TopEdge = oldTrap.TopEdge,
					ContainingFace = oldTrap.ContainingFace
				};
				oldTrap.RightBottom?.LinkTo(oldTrap, right, false);
				oldTrap.RightTop?.LinkTo(oldTrap, right, false);
			}
			else
			{
				right = null;
			}

			TrapNode aboveNode = null, belowNode = null;

			// Create top/bottom trapezoids (which is always necessary)
			if (exceedsLeft)
			{
				// These assumptions need to be met when merging below.  This should all compile out
				// in the release build.
				Debug.Assert(oldTrap.LeftTop == null ^ oldTrap.LeftBottom == null);
				Debug.Assert(lNeighborBottom != null && lNeighborTop != null);

				if (oldTrap.LeftBottom == null)
				{
					// Old trap's left side comes up from vertex below and we're cutting it off so merge top traps
					above = lNeighborTop;
					above.RightVtx = midRightVtx;
					aboveNode = above.Node;

					// Create new bottom trap
					below = new Trapezoid
					{
						RightVtx = midRightVtx,
						LeftVtx = midLeftVtx,
						LeftTop = lNeighborBottom,
						TopEdge = edge,
						BottomEdge = oldTrap.BottomEdge,
						ContainingFace = belowFace
					};
					lNeighborBottom.RightTop = below;
				}
				else
				{
					// Merge bottom traps
					below = lNeighborBottom;
					below.RightVtx = midRightVtx;
					belowNode = below.Node;

					// Create new top trap
					above = new Trapezoid
					{
						RightVtx = midRightVtx,
						LeftVtx = midLeftVtx,
						LeftBottom = lNeighborTop,
						BottomEdge = edge,
						TopEdge = oldTrap.TopEdge,
						ContainingFace = aboveFace
					};
					lNeighborTop.RightBottom = above;
				}
			}
			else if (meetsLeft)
			{
				var leftOfTopTrap = oldTrap.LeftTop;
				var leftOfBottomTrap = oldTrap.LeftBottom;

				above = new Trapezoid
				{
					RightVtx = midRightVtx,
					LeftVtx = midLeftVtx,
					LeftTop = leftOfTopTrap,
					TopEdge = oldTrap.TopEdge,
					BottomEdge = edge,
					ContainingFace = aboveFace
				};

				below = new Trapezoid
				{
					RightVtx = midRightVtx,
					LeftVtx = midLeftVtx,
					LeftBottom = leftOfBottomTrap,
					BottomEdge = oldTrap.BottomEdge,
					TopEdge = edge,
					ContainingFace = belowFace
				};

				oldTrap.LeftTop?.LinkTo(oldTrap, above, true);
				oldTrap.LeftBottom?.LinkTo(oldTrap, below, true);
			}
			else
			{
				// Interior on left - i.e., we created a left trapezoid within the old trapezoid
				above = new Trapezoid
				{
					RightVtx = midRightVtx,
					LeftVtx = midLeftVtx,
					LeftTop = left,
					TopEdge = oldTrap.TopEdge,
					BottomEdge = edge,
					ContainingFace = aboveFace
				};

				below = new Trapezoid
				{
					RightVtx = midRightVtx,
					LeftVtx = midLeftVtx,
					LeftBottom = left,
					BottomEdge = oldTrap.BottomEdge,
					TopEdge = edge,
					ContainingFace = belowFace
				};
				// ReSharper disable once PossibleNullReferenceException
				left.RightTop = above;
				left.RightBottom = below;
			}

			if (meetsRight)
			{
				above.RightTop = oldTrap.RightTop;
				above.RightBottom = null;
				below.RightTop = null;
				below.RightBottom = oldTrap.RightBottom;
				oldTrap.RightTop?.LinkTo(oldTrap, above, false);
				oldTrap.RightBottom?.LinkTo(oldTrap, below, false);
			}
			else if (interiorRight)
			{
				above.RightTop = right;
				above.RightBottom = null;
				below.RightTop = null;
				below.RightBottom = right;
				right.LeftTop = above;
				right.LeftBottom = below;
			}
			// If exceedRight then we'll be backpatched in the next trap to the right.

			if (aboveNode == null)
			{
				aboveNode = new TrapNode(above);
			}

			if (belowNode == null)
			{
				belowNode = new TrapNode(below);
			}

			lNeighborTop = above;
			lNeighborBottom = below;

			// Okay - finally done with the map.  Now to deal with the tree.
			// We always have the edge split to concern ourselves with
			PlacementNode repl = new YNode(edge.InitVertex.Position, edge.NextVertex.Position, aboveNode, belowNode);

			// Do we split off a right trap?
			if (interiorRight)
			{
				repl = new XNode(repl, new TrapNode(right), right.LeftVtx);
				//repl = new XNode(repl, new TrapNode(right), midRight);
			}

			// How about splitting off a left trap?
			if (interiorLeft)
			{
				repl = new XNode(new TrapNode(left), repl, left.RightVtx);
				//repl = new XNode(new TrapNode(left), repl, midLeft);
			}

			return repl;
		}
	}
}
