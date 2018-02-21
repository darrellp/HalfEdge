using System;
using System.Linq;
using System.Text;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
	public struct Vector
	{
		public T X { get; set; }
		public T Y { get; set; }
		public T Z { get; set; }
		public T[] MoreCoordinates;
		public int Rank { get; }

		public Vector(params T[] coordinates)
		{
			Rank = coordinates.Length;
			if (Rank < 2)
			{
				throw new ArgumentException("Rank must be at least two in Vector constructor");
			}

			X = coordinates[0];
			Y = coordinates[1];
			Z = 0;
			MoreCoordinates = null;
			if (Rank >= 3)
			{
				Z = coordinates[2];
				if (Rank > 3)
				{
					MoreCoordinates = new T[Rank - 3];
					Array.Copy(coordinates, 3, MoreCoordinates, 0, Rank - 3);
				}
			}
		}

		public Vector(int rank)
		{
			Rank = rank;
			X = Y = Z = 0;
			MoreCoordinates = null;
			if (rank > 3)
			{
				MoreCoordinates = new T[rank - 3];
			}
		}

		public override string ToString()
		{
			switch (Rank)
			{
				case 2:
					return $"({X}, {Y})";

				case 3:
					return $"({X}, {Y}, {Z})";

				default:
					StringBuilder sb = new StringBuilder("(");
					for (int i = 0; i < Rank; i++)
					{
						sb.Append(this[i].ToString() + (i == Rank - 1 ? "" : ", "));
					}

					sb.Append(")");
					return sb.ToString();
			}
		}

		public Vector Clone()
		{
			var me = this;
			return new Vector(Enumerable.Range(0, Rank).Select(i => me[i]).ToArray());
		}

		public Vector(T x, T y) : this(new T[] {x, y}) { }

		public Vector(T x, T y, T z) : this(new T[] {x, y, z}) { }

		public static Vector CoordinateWise(Vector x1, Vector x2, Func<T, T, T> fn)
		{
			if (x1.Rank != x2.Rank)
			{
				throw new ArgumentException("Operands must have identical ranks");
			}

			var coords = new T[x1.Rank];

			for (int i = 0; i < x1.Rank; i++)
			{
				coords[i] = fn(x1[i], x2[i]);
			}

			return new Vector(coords);
		}

		public static Vector CoordinateWise(Vector x, T scalar, Func<T, T, T> fn)
		{
			var coords = new T[x.Rank];

			for (int i = 0; i < x.Rank; i++)
			{
				coords[i] = fn(x[i], scalar);
			}

			return new Vector(coords);
		}


		public static Vector operator +(Vector x1, Vector x2)
		{
			return CoordinateWise(x1, x2, (v1, v2) => v1 + v2);
		}

		public static Vector operator -(Vector x1, Vector x2)
		{
			return CoordinateWise(x1, x2, (v1, v2) => v1 - v2);
		}

		public static Vector operator *(Vector x, T scalar)
		{
			return CoordinateWise(x, scalar, (v1, v2) => v1 * v2);
		}

		public static Vector operator *(T scalar, Vector x)
		{
			return CoordinateWise(x, scalar, (v1, v2) => v1 * v2);
		}

		public static Vector operator /(Vector x,T  scalar)
		{
			return CoordinateWise(x, scalar, (v1, v2) => v1 / v2);
		}

		public T this[int i]
		{
			get
			{
				switch (i)
				{
					case 0:
						return X;

					case 1:
						return Y;

					case 2:
						return Z;

					default:
						return MoreCoordinates[i - 3];
				}
			}

			set
			{
				switch (i)
				{
					case 0:
						X = value;
						break;

					case 1:
						Y = value;
						break;

					case 2:
						Z = value;
						break;

					default:
						MoreCoordinates[i - 3] = value;
						break;
				}
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Returns a vector 90 degrees in a CCW direction from the original. </summary>
		///
		/// <remarks>	Darrellp, 2/27/2011. </remarks>
		///
		/// <returns>	Vector 90 degrees from original. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public Vector Flip90Ccw()
		{
			return new Vector(-Y, X);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	returns the distance from the origin to the point. </summary>
		///
		/// <remarks>	Darrellp, 2/27/2011. </remarks>
		///
		/// <returns>	Length of the point considered as a vector. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public T Length()
		{
			return Math.Sqrt(X * X + Y * Y);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Returns a normalized version of the point. </summary>
		///
		/// <remarks>	Darrellp, 2/27/2011. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public Vector Normalize()
		{
			var ln = Length();
			// ReSharper disable RedundantCast
			return new Vector(X / (T)ln, Y / (T)ln);
			// ReSharper restore RedundantCast
		}
	}
}
