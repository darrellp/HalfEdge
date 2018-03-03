using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshNav.Placement
{
	class TrapUpdateInfo
	{
		internal Trapezoid OldTrap { get; set; }
		internal Vector LeftPt { get; set; }
		internal Vector RightPt { get; set; }
		internal Vector LeftPtSegment { get; set; }
		internal Vector RightPtSegment { get; set; }
		internal Face Above { get; set; }
		internal Face Below { get; set; }
		internal Trapezoid NeighborTop { get; set; }
		internal Trapezoid NeighborBottom { get; set; }
	}
}
