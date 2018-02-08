using System;
using MathNet.Numerics.LinearAlgebra;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
	internal class RbType : IComparable<RbType>
	{
		private readonly Vector<T> _start;
		private readonly Vector<T> _end;
		// Should be a closure that tracks the sweep line position
		private readonly Func<T> _sweepPos;
		private T _valLast;
		private T _sweepPosLast = T.MinValue;

		public RbType NextHigher { get; set; }
		public RbType NextLower { get; set; }

		public RbType(Vector<double> start, Vector<double> end, Func<double> sweepPos)
		{
			_start = start;
			_end = end;
			_sweepPos = sweepPos;
		}

		public T Value
		{
			get
			{
				var pos = _sweepPos();
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (pos == _sweepPosLast)
				{
					return _valLast;
				}

				_valLast = (pos - _start.X()) / (_end.X() - _start.X()) * (_end.Y() - _start.Y()) + _start.Y();
				_sweepPosLast = pos;
				return _valLast;
			}
		}

		public int CompareTo(RbType other)
		{
			return Value.CompareTo(other.Value);
		}
	}
}
