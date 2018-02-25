using System;
using static MeshNav.Geometry2D;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	The Red Black data type we use in an MeshNavRbTree to determine polygon simpleness. </summary>
	///
	/// <remarks>	Darrell Plank, 2/7/2018. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////
	internal class RbLineSegment : IComparable<RbLineSegment>
	{
		private readonly Vector _left;
		private readonly Vector _right;
		// Should be a closure that tracks the sweep line position
		private readonly Func<T> _sweepPos;
		private T _valLast;
		private T _sweepPosLast = T.MinValue;

		public RbLineSegment NextHigher { get; set; }
		public RbLineSegment NextLower { get; set; }

		public RbLineSegment(Vector left, Vector right, Func<T> sweepPos)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (left.X > right.X || (left.X == right.X && left.Y > right.Y))
			{
				(left, right) = (right, left);
			}
			_left = left;
			_right = right;
			_sweepPos = sweepPos;
		}

		public T Value
		{
			get
			{
				var pos = _sweepPos();
				// ReSharper disable CompareOfFloatsByEqualityOperator
				if (pos == _sweepPosLast)
				{
					return _valLast;
				}

				// TODO: Should we be using CloseEnough() here?
				// Seems possible that they could still cause a NAN in the calculation below if they are close enough.
				if (_right.X == _left.X)
				{
					_valLast = _left.X;
				}
				else
				{
					_valLast = (pos - _left.X) * (_right.Y - _left.Y) / (_right.X - _left.X) + _left.Y;
				}
				// ReSharper restore CompareOfFloatsByEqualityOperator
				_sweepPosLast = pos;
				return _valLast;
			}
		}

		public int CompareTo(RbLineSegment other)
		{
			var ret = Value.CompareTo(other.Value);
			if (ret == 0)
			{
				// If they're the same, then compare slopes
				// ReSharper disable CompareOfFloatsByEqualityOperator
				if (_left.X == _right.X)
				{
					return _left.Y.CompareTo(_right.Y);
				}

				if (other._left.X == other._right.X)
				{
					return other._left.Y.CompareTo(other._right.Y);
				}
				// ReSharper restore CompareOfFloatsByEqualityOperator

				var dxUs = _right.X - _left.X;
				var dyUs = _right.Y - _left.Y;
				var dxThem = other._right.X - other._left.X;
				var dyThem = other._right.Y - other._left.Y;

				ret = (dyUs / dxUs).CompareTo(dyThem / dxThem);
			}

			return ret;
		}

		public bool Intersects(RbLineSegment ls)
		{
			if (ls == null)
			{
				// We don't intersect non-existent segments.
				return false;
			}
			(CrossingType ct, Vector _) = SegSegInt(_left, _right, ls._left, ls._right);
			// TODO: Think about the various crossing types more carefully
			// ReSharper disable PossibleUnintendedReferenceComparison
			return ct == CrossingType.Normal || ct == CrossingType.Edge || ct == CrossingType.Vertex;
			// ReSharper restore PossibleUnintendedReferenceComparison
		}

		public override string ToString()
		{
			return $"({_left.X}, {_left.Y}) - ({_right.X}, {_right.Y})";
		}
	}
}
