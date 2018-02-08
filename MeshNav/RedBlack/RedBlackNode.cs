using System;
using System.Runtime.CompilerServices;
using Common.Collections.Generic;

namespace MeshNav.RedBlack
{
	internal class RedBlackNode<T>
	{
		internal RedBlackNode(T value, Color color = Color.Red)
		{
			Left = null;
			Right = null;
			_height = int.MinValue;
			_value = value;
			SetColor(color);
		}

		#region Properties
		internal virtual T Value
		{
			get => _value;
			set => _value = value;
		}

		/// <summary>
		///     Points to a the left sub tree of this node
		/// </summary>
		internal RedBlackNode<T> Left { get; set; }

		/// <summary>
		///     Points to a the right sub tree of this node
		/// </summary>
		internal RedBlackNode<T> Right { get; set; }

		/// <summary>
		///     The color of the node.
		/// </summary>
		private bool _isRed;

		/// <summary>
		///     Returns true if the node is red, returns false if the node is black.
		/// </summary>
		internal bool IsRed => _isRed;

		/// <summary>
		///     Returns true if this node has no neighbours
		/// </summary>
		internal bool IsLeaf => Left == null && Right == null;

		private int _height;
		private T _value;

		/// <summary>
		///     The height of the node.
		///     O(1) - When cached
		///     O(log n) - Otherwise
		/// </summary>
		internal int Height
		{
			get
			{
				if (_height == int.MinValue) _height = GetNodeHeight();

				return _height;
			}
		}

		/// <summary>
		///     The balance of the node. Node.Balance = Node.Left.Height - Node.Right.Height
		///     O(log n) when one or more heights below this one aren't cached
		///     O(1) otherwise
		/// </summary>
		internal int Balance => GetNodeBalance();

		internal RedBlackNode<T> InOrderPredecessor
		{
			get
			{
				RedBlackNode<T> previous = null;
				var current = Left;
				while (current != null)
				{
					previous = current;
					current = current.Right;
				}

				return previous;
			}
		}

		internal RedBlackNode<T> InOrderSuccessor
		{
			get
			{
				RedBlackNode<T> previous = null;
				var current = Right;
				while (current != null)
				{
					previous = current;
					current = current.Left;
				}

				return previous;
			}
		}

		#endregion

		#region Rotations

		/// <summary>
		///     Rotates the tree rooted at this node in a counter-clockwise manner and recolours the root and pivot nodes
		///     accordingly.
		/// </summary>
		/// <returns>The new root of the tree</returns>
		internal RedBlackNode<T> RotateLeft()
		{
			var pivot = Right;

			Right = pivot.Left;
			pivot.Left = this;

			//fix heights
			pivot.ResetHeight();
			ResetHeight();

			pivot.Left.SetColor(Color.Red);
			pivot.SetColor(Color.Black);

			return pivot;
		}

		/// <summary>
		///     Rotates the tree rooted at this node in a clockwise manner and recolours the root and pivot nodes accordingly.
		/// </summary>
		/// <returns>The new root of the tree</returns>
		internal RedBlackNode<T> RotateRight()
		{
			var pivot = Left;

			Left = pivot.Right;
			pivot.Right = this;

			//fix heights
			pivot.ResetHeight();
			ResetHeight();

			pivot.Right.SetColor(Color.Red);
			pivot.SetColor(Color.Black);

			return pivot;
		}

		#endregion

		#region Height and Balance Helper Methods

		/// <summary>
		///     Returns the height of a node that is potentially null
		/// </summary>
		/// <typeparam name="T">The data type contained within the node</typeparam>
		/// <param name="node">The child node that we want the height for</param>
		/// <returns>-1 if the node is null. Otherwise it returns the cached node height</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetChildNodeHeight(RedBlackNode<T> node)
		{
			return node?.Height ?? -1;
		}

		/// <summary>
		///     Retrieves the height of the node calculated as Height = Max(Left.Height, Right.Height) + 1
		///     The height of a tree that only has a root is 0
		/// </summary>
		/// <typeparam name="T">The data type contained within the node</typeparam>
		internal int GetNodeHeight()
		{
			return Math.Max(GetChildNodeHeight(Left), GetChildNodeHeight(Right)) + 1;
		}

		/// <summary>
		///     Retrieves the balance of a node calculated as node.Balance = Node.Left.Balance - Node.Right.Balance
		/// </summary>
		internal int GetNodeBalance()
		{
			return GetChildNodeHeight(Left) - GetChildNodeHeight(Right);
		}

		#endregion

		#region Methods

		internal void SetIsRed(bool isRed)
		{
			_isRed = isRed;
		}

		internal void SetColor(Color color)
		{
			_isRed = color == Color.Red;
		}

		internal void ResetHeight()
		{
			_height = int.MinValue;
		}

		public override string ToString()
		{
			var format = "{0},{1}; Left={2}; Right={3}";

			var left = Left?.Value.ToString() ?? "null";
			var right = Right?.Value.ToString() ?? "null";

			return string.Format(format, Value.ToString(), IsRed ? "Red" : "Black", left, right);
		}

		#endregion
	}
}