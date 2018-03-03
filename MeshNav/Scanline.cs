using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

namespace MeshNav
{
    public class Scanline<TEvent, TInput, TScan>
    {
        private readonly BinaryPriorityQueue<TScan> _pq;
        private readonly IEnumerator<TScan> _events;

        public int ScanCount => _pq.Count;

        public static IEnumerable<TEvent> Events(IEnumerable<TInput> input, Func<TInput, TEvent> projection, Func<TInput, TInput, int> compare = null)
        {
            var inList = input.ToList();
            if (compare == null)
            {
                inList.Sort();
            }
            else
            {
                inList.Sort((i1, i2) => compare(i1, i2));
            }
            return inList.Select(projection);
        }

        public static IEnumerable<TInput> Events(IEnumerable<TInput> input, Comparison<TInput> compare = null)
        {
            var inList = input.ToList();
            if (compare == null)
            {
                inList.Sort();
            }
            else
            {
                inList.Sort(compare);
            }
            return inList;
        }

        public Scanline(IEnumerable<TEvent> events, Func<TEvent, TScan> fnScanItem, Func<TScan, TScan, int> compare = null)
        {
            _pq = new BinaryPriorityQueue<TScan>(compare);
            _events = events.Select(fnScanItem).GetEnumerator();
        }

        public bool PullEvent()
        {
            if (!_events.MoveNext())
            {
                return false;
            }
            _pq.Add(_events.Current);
            return true;
        }

        public TScan PopScan()
        {
            return ScanCount == 0 ? default : _pq.Pop();
        }
    }
}
