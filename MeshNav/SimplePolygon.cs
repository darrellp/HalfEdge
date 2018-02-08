using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Priority_Queue;
using static MeshNav.Geometry2D;
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
            var firstPt = poly.FirstOrDefault();
            if (firstPt == null)
            {
                return true;
            }
            if (firstPt.Count() != 2)
            {
                throw new MeshNavException("Calling FTestSimplePolygon with non-2D points");
            }

            var _edgeList = new List<LineSegment>();
            var eventQueue = new BinaryPriorityQueue<SimplePolygonEvent>((e1, e2) => cmpVectors(e1.Vertex, e2.Vertex));

            var polyList = poly.ToList();
            for (var iVtx = 0; iVtx < polyList.Count; iVtx++)
            {
                var vtxLow = polyList[iVtx];
                var vtxHigh = polyList[(iVtx + 1) % polyList.Count];
                var cmp = cmpVectors(vtxLow, vtxHigh);
                if (cmp == 0)
                {
                    // We ignore zero length sides
                    continue;
                }
                if (cmp > 0)
                {
                    (vtxLow, vtxHigh) = (vtxHigh, vtxLow);
                }
                _edgeList.Add(new LineSegment(vtxLow, vtxHigh));
                eventQueue.Add(new SimplePolygonEvent(vtxLow, iVtx, cmpVectors(vtxLow, vtxHigh) == 1));
            }

            var segTable = new SegTable();
            while (eventQueue.Count != 0)
            {
                var nextEvent = eventQueue.Pop();
                if (nextEvent.IsRightEndpoint)
                {
                    // We have to remove the corresponding line segment from consideration
                    segTable.RemoveSegment(_edgeList[nextEvent.SegIndex]);
                }
                else
                {
                    // Insert the corresponding line segment
                    (Interval low, Interval high) = segTable.Insert(nextEvent.Vertex.Y(), nextEvent.SegIndex);
                    var lsCur = _edgeList[nextEvent.SegIndex];
                    if (lsCur.Intersects(_edgeList[high.LowSegmentIndex]) ||
                        lsCur.Intersects(_edgeList[low.HighSegmentIndex]))
                    {
                        return true;
                    }
                }
            }
            return true;

            int cmpVectors(Vector<T> v1, Vector<T> v2)
            {
                var cmp = v1.X().CompareTo(v2.X());
                if (cmp == 0)
                {
                    cmp = v1.Y().CompareTo(v2.Y());
                }
                return cmp;
            }
        }

        private struct LineSegment
        {
            private Vector<T> LowPoint { get; }
            private Vector<T> HighPoint { get; }

            public LineSegment(Vector<double> lowPoint, Vector<double> highPoint)
            {
                LowPoint = lowPoint;
                HighPoint = highPoint;
            }

            public bool Intersects(LineSegment ls)
            {
                (CrossingType ct, Vector<T> pt) = SegSegInt(LowPoint, HighPoint, ls.LowPoint, ls.HighPoint);
                // TODO: Think about the various crossing types more carefully
                return ct == CrossingType.Normal;
            }
        }

        private struct Interval
        {
            public int LowSegmentIndex { get; }
            public int HighSegmentIndex { get; }
            private readonly T _low;
            private readonly T _high;

            private Interval(T low, int lowSegmentIndex, T high, int highSegmentIndex)
            {
                _low = low;
                LowSegmentIndex = lowSegmentIndex;
                _high = high;
                HighSegmentIndex = highSegmentIndex;
            }

            (Interval, Interval) Split(T val, int segmentIndex)
            {
                if (val < _low || val > _high)
                {
                    throw new MeshNavException("Internal error in Interval class");
                }
                return (new Interval(_low, LowSegmentIndex, val, segmentIndex), new Interval(val, segmentIndex, _high, HighSegmentIndex));
            }
        }
        private class SegTable
        {
            public void RemoveSegment(LineSegment segment)
            {
            }

            public (Interval, Interval) Insert(T val, int segmentIndex)
            {
                throw new NotImplementedException();
            }
        }

        private struct SimplePolygonEvent
        {
            internal Vector<T> Vertex;
            internal int SegIndex;
            internal bool IsRightEndpoint;

            public SimplePolygonEvent(Vector<double> vertex, int segIndex, bool isRightEndpoint)
            {
                Vertex = vertex;
                SegIndex = segIndex;
                IsRightEndpoint = isRightEndpoint;
            }
        }
    }
}
