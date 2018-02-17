﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav.Placement
{
	internal class Trapezoid
	{
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		internal bool IsBoundary => TopEdge.InitVertex.Y == T.MaxValue || BottomEdge.InitVertex.Y == T.MinValue;
		internal bool IsBBox { get; private set; }
		internal HalfEdge TopEdge { get; set; }
		internal HalfEdge BottomEdge { get; set; }
		internal Vertex LeftVtx { get; set; }
		internal Vertex RightVtx { get; set; }
		internal Trapezoid RightTop { get; set; }
		internal Trapezoid RightBottom { get; set; }
		internal Trapezoid LeftTop { get; set; }
		internal Trapezoid LeftBottom { get; set; }

		internal T Left => LeftVtx.Position.X();
		internal T Right => RightVtx.Position.X();

		internal Face ContainingFace { get; set; }

		internal TrapNode Node { get; set; }

		internal Trapezoid() { }

		private static Trapezoid _bbox;

		internal static Trapezoid Bbox()
		{
			if (_bbox != null)
			{
				return _bbox;
			}
			// Throwaway mesh to create bounding box in
			var mesh = new Factory(2).CreateMesh();

			// ReSharper disable InconsistentNaming
			var ptLL = mesh.AddVertex(T.MinValue, T.MinValue);
			var ptLR = mesh.AddVertex(T.MaxValue, T.MinValue);
			var ptUL = mesh.AddVertex(T.MinValue, T.MaxValue);
			var ptUR = mesh.AddVertex(T.MaxValue, T.MaxValue);
			// ReSharper restore InconsistentNaming

			mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
			_bbox = new Trapezoid
			{
				TopEdge = ptUL.Edge,
				BottomEdge = ptLR.Edge,
				LeftVtx = ptUL,
				RightVtx = ptUR,
				IsBBox = true
			};
			return _bbox;
		}

		internal void RelinkNeighbors(Trapezoid newGuy, bool doLinkToLeftNeighbor)
		{
			if (doLinkToLeftNeighbor)
			{
				LeftTop?.LinkTo(this, newGuy, true);
				LeftBottom?.LinkTo(this, newGuy, true);
			}
			else
			{
				RightTop?.LinkTo(this, newGuy, false);
				RightBottom?.LinkTo(this, newGuy, false);
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Links to a new trapezoid neighbor. </summary>
		///
		/// <remarks>	Note - this is a one way link, altering only the values in this trapezoid.
		/// 			The links in the newGuy are untouched.  
		/// 			Darrell Plank, 2/12/2018. </remarks>
		///
		/// <param name="oldTrap">		  	The old trapezoid we're replacing as a neighbor. </param>
		/// <param name="newGuy">		  	The new trapezoid we're linking to. </param>
		/// <param name="isNewGuyOnRight">	True if the new trapezoid is being linked on right. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal void LinkTo(Trapezoid oldTrap, Trapezoid newGuy, bool isNewGuyOnRight)
		{
			if (isNewGuyOnRight)
			{
				if (oldTrap == RightTop)
				{
					RightTop = newGuy;
				}
				else if (oldTrap == RightBottom)
				{
					RightBottom = newGuy;
				}
			}
			else
			{
				if (oldTrap == LeftTop)
				{
					LeftTop = newGuy;
				}
				else if (oldTrap == LeftBottom)
				{
					LeftBottom = newGuy;
				}
			}
		}

		public override string ToString()
		{
			return $"{Left} - {Right} le {BottomEdge} ue {TopEdge}";
		}
	}
}