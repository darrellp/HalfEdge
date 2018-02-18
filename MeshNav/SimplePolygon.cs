using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Priority_Queue;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
    class SimplePolygon
    {

        public static bool FTestSimplePolygon(IEnumerable<Vector<T>> poly)
        {
            var polyList = poly?.ToList();
	        var sweep = T.MinValue;

	        if (polyList == null)
	        {
				throw new ArgumentException($"poly is null in {nameof(FTestSimplePolygon)}");
	        }

            if (polyList.Count == 0)
            {
                return true;
            }

            if (polyList[0].Count != 2)
            {
                throw new ArgumentException("Calling FTestSimplePolygon with non-2D points");
            }

            var edgeList = new List<RbLineSegment>();
            var eventQueue = new BinaryPriorityQueue<SimplePolygonEvent>((e1, e2) => CmpVectors(e1.Vertex, e2.Vertex));
            for (var iVtx = 0; iVtx < polyList.Count; iVtx++)
            {
                var vtxLow = polyList[iVtx];
                var vtxHigh = polyList[(iVtx + 1) % polyList.Count];
                var cmp = CmpVectors(vtxLow, vtxHigh);
                if (cmp == 0)
                {
                    // We ignore zero length sides
                    continue;
                }
                if (cmp > 0)
                {
                    (vtxLow, vtxHigh) = (vtxHigh, vtxLow);
                }
	            // ReSharper disable once AccessToModifiedClosure
                edgeList.Add(new RbLineSegment(vtxLow, vtxHigh, () => sweep));
                eventQueue.Add(new SimplePolygonEvent(vtxLow, iVtx, false));
	            eventQueue.Add(new SimplePolygonEvent(vtxHigh, iVtx, true));
            }

			var segTable = new MeshNavRbTree();
            while (eventQueue.Count != 0)
            {
                var nextEvent = eventQueue.Pop();
	            Console.WriteLine(nextEvent);
                if (nextEvent.IsRightEndpoint)
                {
                    // We have to remove the corresponding line segment from consideration
                    segTable.DeleteBracketed(edgeList[nextEvent.SegIndex]);
                }
                else
                {
	                sweep = nextEvent.Vertex.X();
                    var lsCur = edgeList[nextEvent.SegIndex];

					// Insert the corresponding line segment
					(RbLineSegment high, RbLineSegment low) = segTable.InsertBracketed(lsCur);
                    if (lsCur.Intersects(low) || lsCur.Intersects(high))
                    {
                        return false;
                    }
                }
            }
            return true;

            int CmpVectors(Vector<T> v1, Vector<T> v2)
            {
                var cmp = v1.X().CompareTo(v2.X());
                if (cmp == 0)
                {
                    cmp = v1.Y().CompareTo(v2.Y());
                }
                return cmp;
            }
        }

	    private struct SimplePolygonEvent
        {
            internal readonly Vector<T> Vertex;
            internal readonly int SegIndex;
            internal readonly bool IsRightEndpoint;

            public SimplePolygonEvent(Vector<double> vertex, int segIndex, bool isRightEndpoint)
            {
                Vertex = vertex;
                SegIndex = segIndex;
                IsRightEndpoint = isRightEndpoint;
            }

	        public override string ToString()
	        {
		        return $"({Vertex.X()}, {Vertex.Y()}) in seg index {SegIndex} : {(IsRightEndpoint ? "Right" : "Left")}";
	        }
        }
    }
}
