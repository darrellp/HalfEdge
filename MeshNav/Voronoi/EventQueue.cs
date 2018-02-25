using System.Collections.Generic;
using Priority_Queue;

namespace DAP.CompGeom
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Queue of fortune events. </summary>
	/// <remarks>	Darrellp, 2/18/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////
	internal class EventQueue : BinaryQueueWithDeletions<FortuneEvent>
	{
		#region Constructor
		internal EventQueue()
		{
			CircleEvents = new LinkedList<CircleEvent>();
		}
		#endregion

		#region Circle event special handling
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Adds a circle event. </summary>
		/// <remarks>	Darrellp, 2/21/2011. </remarks>
		/// <param name="cevt">	The event to add. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal void AddCircleEvent(CircleEvent cevt)
		{
			// Add to the priority queue
			Add(cevt);

			// We also have to add them to our linked list of circle events
			CircleEvents.AddFirst(cevt);

			// Keeping track of their LinkedListNode allows us to delete them efficiently later
			cevt.LinkedListNode = CircleEvents.First;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the circle events. </summary>
		/// <value>	The circle events. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal LinkedList<CircleEvent> CircleEvents { get; }
		#endregion
	}
}