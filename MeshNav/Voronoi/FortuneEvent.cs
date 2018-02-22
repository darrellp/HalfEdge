using System;
using System.Collections.Generic;
using Priority_Queue;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	
	/// A class for the events that drive the Fortune algorithm 
	/// </summary>
	///
	/// <remarks>	
	/// The fortune algorithm works by moving a sweepline down through the sites of the diagram.
	/// During that movement, events are added, removed and popped from a priority queue.  Those
	/// events each have a y coordinate and the event with the largest y coordinate is popped out of
	/// the priority queue.  These events are of two types: circle events (CircleEvent) and site
	/// events (SiteEvent).  FortuneEvent is an abstract class which serves as the base for both
	/// these types of event.
	/// 
	/// Darrellp, 2/21/2011. 
	/// </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	internal abstract class FortuneEvent : ILocatable
	{
		#region Private Variables
		int _i = -1;				// Index we maintain for the IPriorityQueueElement operations
		// and the location of the site for site events.
		#endregion

		#region Properties

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the point where this event occurs. </summary>
		///
		/// <value>	The point where this event occurs. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal PointD Pt { get; set; }

		#endregion

		#region Constructor
		internal FortuneEvent(PointD pt)
		{
			Pt = pt;
		}
		#endregion

		#region PriorityQueueElement Members
		int ILocatable.Index
		{
			get => _i;
		    set { _i = value; }
		}
		#endregion

		#region IComparable Members
		/// <summary>
		/// Compare two events.  We order them using y coordinate first and then x coordinate.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			// Get our PointD
			var ptCompare = ((FortuneEvent)obj).Pt;

			// If two events have essentially the same Y coordinate, we defer to the X coordinate
			if (Geometry.FCloseEnough(Pt.Y, ptCompare.Y))
			{
				if (Pt.X > ptCompare.X)
				{
					return 1;
				}
				if (Pt.X < ptCompare.X)
				{
					return -1;
				}
				// If we are a site event and the compared object is a circle event
				if (GetType() == typeof(SiteEvent) && obj.GetType() == typeof(CircleEvent))
				{
					// Site events are bigger than circle events at the same point

					return 1;
				}
				return 0;
			}
			if (Pt.Y > ptCompare.Y)
			{
				return 1;
			}
			return -1;
		}
		#endregion

		#region Abstract handler
		/// <summary>
		/// Handle the event
		/// </summary>
		/// <param name="fortune">Fortune data structure being built</param>
		internal abstract void Handle(Fortune fortune);
		#endregion

		#region Circle creation

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Creates a circle event. </summary>
		///
		/// <remarks>Circle events are created at the circumcenters of three sites - the sites for poly1/2/3.</remarks>
		/// <param name="poly1">		The first polygon. </param>
		/// <param name="poly2">		The second polygon. </param>
		/// <param name="poly3">		The third polygon. </param>
		/// <param name="yScanLine">	The y coordinate scan line. </param>
		///
		/// <returns>	A new circle event. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal static CircleEvent CreateCircleEvent(FortunePoly poly1, FortunePoly poly2, FortunePoly poly3, double yScanLine)
		{
			// Locals
		    CircleEvent cevtRet = null;

			// Determine a circumcenter for the sites of poly1/2/3.
			if (Geometry.FFindCircumcenter(poly1.VoronoiPoint, poly2.VoronoiPoint, poly3.VoronoiPoint, out var ptCenter))
			{
				// Determine y coordinate for the side of the circle
				// The event will fire when the scan line hits that y position
				var radius = Geometry.Distance(poly1.VoronoiPoint, ptCenter);
				ptCenter.Y -= radius;

				// If the circumcenter is above the scan line we've already passed it by, so don't put it in the queue
				if (ptCenter.Y <= yScanLine)
				{
					cevtRet = new CircleEvent(ptCenter, radius);
				}
			}

			return cevtRet;
		}
		#endregion
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	
	/// Site events are inserted into the priority queue when the fortune object is created. See its
	/// constructor for details. 
	/// </summary>
	///
	/// <remarks>	Darrellp, 2/21/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	internal class SiteEvent : FortuneEvent
	{
		#region Properties

		internal FortunePoly Poly { get; set; }

		#endregion

		#region Constructor
		internal SiteEvent(FortunePoly poly) : base(poly.VoronoiPoint)
		{
			Poly = poly;
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return $"SiteEvent: Gen = {Poly.Index}, Pt = ({Pt.X}, {Pt.Y})";
		}
		#endregion

		#region Event handling
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Handle a site event.  This is done by adding a polgyon/parabola to the beachline. </summary>
		///
		/// <remarks>	Darrellp, 2/21/2011. </remarks>
		///
		/// <param name="fortune">	The fortune object to update. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal override void Handle(Fortune fortune)
		{
			// Insert the new parabola into the beachline
			fortune.Bchl.PolyInsertNode(this, fortune.QevEvents);
		}
		#endregion
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Circle events snuff out a parabola on the beachline. </summary>
	///
	/// <remarks>	Darrellp, 2/21/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	internal class CircleEvent : FortuneEvent
	{
		#region Private Variables
		private readonly double _radius;						// Radius of the circle
		private readonly double _radiusSq;						// Square of circle radius

		#endregion

		#region Properties
		internal LinkedListNode<CircleEvent> LinkedListNode { get; set; }
		internal bool FZeroLength { get; set; }

		internal LeafNode LfnEliminated { get; set; }

		internal PointD VoronoiVertex => new PointD(Pt.X, Pt.Y + _radius);

	    #endregion

		#region Constructor
		internal CircleEvent(PointD pt, double radius) : base(pt)
		{
			_radius = radius;
			_radiusSq = radius*radius;
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return $"CircleEvent: Gen = {LfnEliminated.Poly.Index}, vtx = ({VoronoiVertex.X}, {VoronoiVertex.Y}), yscl = {Pt.Y}";
		}
		#endregion

		#region Event handling

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Handle a circle event.  Snuff out the a parabola, form a vertex of the diagram. </summary>
		///
		/// <remarks>	Darrellp, 2/21/2011. </remarks>
		///
		/// <param name="fortune">	The fortune. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal override void Handle(Fortune fortune)
		{
			// Remove a parabola node from the beachline since it's being squeezed out.  Insert a vertex into
			// the voronoi diagram
			fortune.Bchl.RemoveNodeAndInsertVertex(this, LfnEliminated, VoronoiVertex, fortune.QevEvents);
		}
		#endregion

		#region Queries

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Returns true if the circle event contains the passed in point. </summary>
		///
		/// <remarks>	
		/// Darrell Plank, 2/21/2011. 
		/// </remarks>
		///
		/// <param name="pt">	Point to check. </param>
		///
		/// <returns>	True if its contained in the circle, else false. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal bool Contains(PointD pt)
		{
			return Geometry.DistanceSq(pt, Pt) <= _radiusSq;
		}
		#endregion
	}
}
