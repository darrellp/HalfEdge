﻿using System;
using System.Collections.Generic;
using Common.Collections.Generic;

namespace MeshNav.RedBlack
{
	// Mostly lifted from the excellect code at https://github.com/ArsenShnurkov/Common
	public class RedBlackTree<T> where T : IComparable<T>
	{
		#region Constructors
		public RedBlackTree()
		{
			Root = null;
			Count = 0;
		}
		#endregion

		#region Public Methods
		public void AssertValidTree()
		{
			if (Root != null)
			{
				RedBlackNode<T> previousNode = null;
				foreach (var node in InOrderNodeIterator)
				{
					if (previousNode != null && previousNode.Value.CompareTo(node.Value) >= 0)
					{
						throw new MeshNavException("Invalid Red Black Tree");
					}

					previousNode = node;
				}
			}

			AssertValidTree(Root, out _);
		}
		#endregion

		#region Properties
		internal RedBlackNode<T> Root { get; set; }

		/// <summary>
		///     Returns the number of nodes in the tree
		/// </summary>
		public int Count { get; internal set; }

		/// <summary>
		///     The height of the tree. Returns -1 for an empty tree.
		/// </summary>
		public int Height
		{
			get
			{
				if (Root == null)
				{
					return -1;
				}

				return Root.Height;
			}
		}

		/// <summary>
		///     Returns the balance of the tree defined as Balance = LeftTree.Height - Right.Height
		/// </summary>
		public int Balance => Root?.Balance ?? 0;
		#endregion

		#region Depth
		/// <summary>
		///     Finds the depth of the node with the given value.
		///     Throws a TreeNotRootedException if the tree is empty
		///     Throws a NodeNotFoundException if the node was not found
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Depth(T value)
		{
			return Depth(Root, value);
		}

		/// <summary>
		///     Finds the depth of the node with the given value underneath the given root
		///     Throws a NodeNotFoundException if the node was not found
		/// </summary>
		/// <param name="root">The root of the tree to search</param>
		/// <param name="value">The value of the node whose depth should be returned</param>
		/// <returns></returns>
		private static int Depth(RedBlackNode<T> root, T value)
		{
			if (root == null)
			{
				throw new MeshNavException("No root in RedBlackTree.Depth()");
			}

			var compareResult = root.Value.CompareTo(value);
			int result;

			if (compareResult > 0)
			{
				result = Depth(root.Left, value) + 1;
			}
			else if (compareResult < 0)
			{
				result = Depth(root.Right, value) + 1;
			}
			else
			{
				result = 0;
			}

			return result;
		}
		#endregion

		#region Insert
		/// <summary>
		///     Inserts the given value into the tree and ensures that the tree is rebalanced according to the red-black tree rules
		///     Throws a ArgumentException if the value already exists in the tree
		///     O(log n)
		/// </summary>
		public void Insert(T value)
		{
			Root = Insert(Root, new RedBlackNode<T>(value));

			if (Root.IsRed)
			{
				Root.SetColor(Color.Black);
			}
		}

		/// <summary>
		///     Inserts the given node underneath the given root according to the BinarySearchTree algorithm and then
		///     rebalances the tree according to the red-black tree rules
		/// </summary>
		/// <param name="root">The root node of the tree</param>
		/// <param name="node">The node to insert</param>
		/// <returns>The new root of the tree as it may have changed</returns>
		private RedBlackNode<T> Insert(RedBlackNode<T> root, RedBlackNode<T> node)
		{
			if (root == null)
			{
				root = node;
				++Count;
			}
			else
			{
				root.ResetHeight();
				var compareResult = root.Value.CompareTo(node.Value);
				if (compareResult > 0)
				{
					root.Left = Insert(root.Left, node);
				}
				else if (compareResult < 0)
				{
					root.Right = Insert(root.Right, node);
				}
				else
				{
					throw new ArgumentException("Inserting duplicate in Red Black tree");
				}
			}

			root = PostInsertCleanup(root, node);

			return root;
		}

		internal RedBlackNode<T> PostInsertCleanup(RedBlackNode<T> root, RedBlackNode<T> node)
		{
			root = root.Value.CompareTo(node.Value) > 0
				? Insert_Case1_LeftTwoRedChidren(root)
				: Insert_Case1_RightTwoRedChidren(root);
			return root;
		}

		/// <summary>
		///     Deals with the case where after inserting a node to the left, a node has two red children. If this case is not
		///     matched we move on to case 2
		/// </summary>
		private RedBlackNode<T> Insert_Case1_LeftTwoRedChidren(RedBlackNode<T> root)
		{
			if (IsNodeRed(root.Left) && (IsNodeRed(root.Left.Left) || IsNodeRed(root.Left.Right)))
			{
				if (IsNodeRed(root.Right))
				{
					MoveBlackDown(root);
				}
				else
				{
					root = Insert_Case2_TwoLeftReds(root);
				}
			}

			return root;
		}

		/// <summary>
		///     Deals with the case where after inserting a node to the right, a node has two red children. If this case is not
		///     matched we move on to case 2
		/// </summary>
		private RedBlackNode<T> Insert_Case1_RightTwoRedChidren(RedBlackNode<T> root)
		{
			if (IsNodeRed(root.Right) && (IsNodeRed(root.Right.Right) || IsNodeRed(root.Right.Left)))
			{
				if (IsNodeRed(root.Left))
				{
					MoveBlackDown(root);
				}
				else
				{
					root = Insert_Case2_TwoRightReds(root);
				}
			}

			return root;
		}

		/// <summary>
		///     Deals with the case where after an insert to the left we have two red nodes as left sub-children of each-other
		/// </summary>
		private RedBlackNode<T> Insert_Case2_TwoLeftReds(RedBlackNode<T> root)
		{
			if (IsNodeRed(root.Left.Left))
			{
				root = root.RotateRight();
			}
			else if (IsNodeRed(root.Left.Right))
			{
				root = Insert_Case3_LeftRightReds(root);
			}

			return root;
		}

		/// <summary>
		///     Deals with the case where after an insert to the right we have two red nodes as right sub-children of each-other
		/// </summary>
		private RedBlackNode<T> Insert_Case2_TwoRightReds(RedBlackNode<T> root)
		{
			if (IsNodeRed(root.Right.Right))
			{
				root = root.RotateLeft();
			}
			else if (IsNodeRed(root.Right.Left))
			{
				root = Insert_Case3_RightLeftReds(root);
			}

			return root;
		}

		/// <summary>
		///     Deals with the case where after inserting a node to the right we have a red node as a right sub-child of its parent
		///     and has another red node as a left subchild.
		/// </summary>
		private RedBlackNode<T> Insert_Case3_RightLeftReds(RedBlackNode<T> root)
		{
			root.Right = root.Right.RotateRight();
			root = root.RotateLeft();

			return root;
		}

		/// <summary>
		///     Deals with the case where after inserting a node to the left we have a red node as a left sub-child of its parent
		///     and has another red node as a right subchild.
		/// </summary>
		private RedBlackNode<T> Insert_Case3_LeftRightReds(RedBlackNode<T> root)
		{
			root.Left = root.Left.RotateLeft();
			root = root.RotateRight();

			return root;
		}
		#endregion

		#region Delete
		/// <summary>
		///     Deletes the given value from the tree.
		///     Throws a NodeNotFoundException if the value doesnt exists in the tree
		///     Throws a TreeNotRootedException if the tree is empty
		///     O(log n)
		/// </summary>
		public bool Delete(T value)
		{
			var result = true;
			try
			{
				if (Root == null)
				{
					result = false;
				}
				else
				{
					var done = false;
					Root = Delete(Root, value, ref done);

					Root?.SetColor(Color.Black);
				}
			}
			catch (MeshNavException)
			{
				result = false;
			}

			return result;
		}

		/// <summary>
		///     Deletes the given value from the tree at the given root and ensures red-black tree properties are maintained ny
		///     recolouring nodes and rotations
		/// </summary>
		/// <param name="root">The root node of the tree</param>
		/// <param name="value">The value to delete</param>
		/// <param name="done">A flag determining if more rebalancing and recolouring is necessary</param>
		/// <returns>The new root of the tree as it may have changed</returns>
		private RedBlackNode<T> Delete(RedBlackNode<T> root, T value, ref bool done)
		{
			var compareResult = root.Value.CompareTo(value);
			if (compareResult == 0)
			{
				if (root.Left != null && root.Right != null)
				{
					compareResult = 1;
					root.Value = root.InOrderPredecessor.Value;
					value = root.Value;
					root.ResetHeight();
				}
				// Node only has left child
				else if (root.Left != null)
				{
					--Count;
					root.Left.ResetHeight();
					if (IsNodeRed(root.Left)) // node to delete is black but has red child that can be recoloured
					{
						root.Left.SetColor(Color.Black);
						done = true;
					}

					root = root.Left;
				}
				// Node only has Right child
				else if (root.Right != null)
				{
					--Count;
					root.Right.ResetHeight();
					if (IsNodeRed(root.Right)) // node to delete is black but has red child that can be recoloured
					{
						root.Right.SetColor(Color.Black);
						done = true;
					}

					root = root.Right;
				}
				else // deleting leaf - done if red
				{
					--Count;
					done = IsNodeRed(root);
					root = null;
				}
			}

			// ReSharper disable PossibleNullReferenceException
			if (compareResult > 0)
			{
				if (root.Left != null)
				{
					root.Left = Delete(root.Left, value, ref done);
					if (!done)
					{
						root = DeleteRebalanceLeft(root, ref done);
					}
				}
				else
				{
					throw new MeshNavException("Value not found");
				}
			}
			else if (compareResult < 0)
			{
				if (root.Right != null)
				{
					root.Right = Delete(root.Right, value, ref done);
					if (!done)
					{
						root = DeleteRebalanceRight(root, ref done);
					}
				}
				else
				{
					throw new MeshNavException("Value not found");
				}
			}
			// ReSharper restore PossibleNullReferenceException

			return root;
		}

		/// <summary>
		///     Rebalances the root after a black node was deleted in the right sub-tree
		/// </summary>
		/// <param name="root"></param>
		/// <param name="done"></param>
		/// <returns></returns>
		private RedBlackNode<T> DeleteRebalanceRight(RedBlackNode<T> root, ref bool done)
		{
			var parent = root;
			var sibling = root.Left;

			// Rotation to reduce the red sibling case to the easier to handle black sibling case
			if (IsNodeRed(sibling))
			{
				root = root.RotateRight();
				sibling = parent.Left;
			}

			if (sibling != null)
			{
				if (!IsNodeRed(sibling.Left) && !IsNodeRed(sibling.Right))
				{
					if (IsNodeRed(parent))
					{
						done = true;
					}

					parent.SetColor(Color.Black);
					sibling.SetColor(Color.Red);
				}
				else
				{
					var parentIsRed = parent.IsRed;
					var sameRoot = root == parent;

					if (IsNodeRed(sibling.Left))
					{
						parent = parent.RotateRight();
					}
					else
					{
						parent.Left = parent.Left.RotateLeft();
						parent = parent.RotateRight();
					}

					parent.SetIsRed(parentIsRed);
					parent.Left.SetColor(Color.Black);
					parent.Right.SetColor(Color.Black);

					if (sameRoot)
					{
						root = parent;
					}
					else
					{
						root.Right = parent;
					}

					done = true;
				}
			}

			return root;
		}

		private RedBlackNode<T> DeleteRebalanceLeft(RedBlackNode<T> root, ref bool done)
		{
			// Rotation to reduce the red sibling case to the easier to handle black sibling case
			var parent = root;
			var sibling = root.Right;

			if (IsNodeRed(sibling))
			{
				root = root.RotateLeft();
				sibling = parent.Right;
			}

			if (sibling != null)
			{
				if (!IsNodeRed(sibling.Left) && !IsNodeRed(sibling.Right))
				{
					if (IsNodeRed(parent))
					{
						done = true;
					}

					parent.SetColor(Color.Black);
					sibling.SetColor(Color.Red);
				}
				else
				{
					var parentIsRed = parent.IsRed;
					var sameRoot = root == parent;

					if (IsNodeRed(sibling.Right))
					{
						parent = parent.RotateLeft();
					}
					else
					{
						parent.Right = parent.Right.RotateRight();
						parent = parent.RotateLeft();
					}

					parent.SetIsRed(parentIsRed);
					parent.Left.SetColor(Color.Black);
					parent.Right.SetColor(Color.Black);

					if (sameRoot)
					{
						root = parent;
					}
					else
					{
						root.Left = parent;
					}

					done = true;
				}
			}

			return root;
		}
		#endregion

		#region Helper Methods
		/// <summary>
		///     Returns true if the node is red. Returns false if the node is black or null.
		/// </summary>
		private static bool IsNodeRed(RedBlackNode<T> node)
		{
			return node != null && node.IsRed;
		}

		/// <summary>
		///     Moves a black root node down to it's two children and colours the root red
		/// </summary>
		/// <param name="root"></param>
		private static void MoveBlackDown(RedBlackNode<T> root)
		{
			root.SetColor(Color.Red);
			root.Left.SetColor(Color.Black);
			root.Right.SetColor(Color.Black);
		}

		private static void AssertValidTree(RedBlackNode<T> root, out int numBlack)
		{
			if (root == null)
			{
				numBlack = 0;
			}
			else
			{
				if (IsNodeRed(root))
				{
					if (IsNodeRed(root.Left) || IsNodeRed(root.Right))
					{
						throw new MeshNavException("Invalid Red Black Tree");
					}
				}

				AssertValidTree(root.Left, out var leftBlack);
				AssertValidTree(root.Right, out var rightBlack);

				if (leftBlack != rightBlack)
				{
					throw new MeshNavException("Invalid Red Black Tree");
				}

				if (IsNodeRed(root))
				{
					numBlack = leftBlack;
				}
				else
				{
					numBlack = leftBlack + 1;
				}
			}
		}
		#endregion

		#region Node Iterators
		private IEnumerable<RedBlackNode<T>> InOrderNodeIterator
		{
			get
			{
				var current = Root;
				var parentStack = new Stack<RedBlackNode<T>>();

				while (current != null || parentStack.Count != 0)
				{
					if (current != null)
					{
						parentStack.Push(current);
						current = current.Left;
					}
					else
					{
						current = parentStack.Pop();
						yield return current;
						current = current.Right;
					}
				}
			}
		}

		private IEnumerable<RedBlackNode<T>> PostOrderNodeIterator
		{
			get
			{
				RedBlackNode<T> previous = null;
				var nodeStack = new Stack<RedBlackNode<T>>();

				if (Root != null)
				{
					nodeStack.Push(Root);
				}

				while (nodeStack.Count > 0)
				{
					var current = nodeStack.Peek();
					if (previous == null || previous.Left == current || previous.Right == current)
					{
						if (current.Left != null)
						{
							nodeStack.Push(current.Left);
						}
						else if (current.Right != null)
						{
							nodeStack.Push(current.Right);
						}
					}
					else if (current.Left == previous)
					{
						if (current.Right != null)
						{
							nodeStack.Push(current.Right);
						}
					}
					else
					{
						yield return current;
						nodeStack.Pop();
					}

					previous = current;
				}
			}
		}

		private IEnumerable<RedBlackNode<T>> PreOrderNodeIterator
		{
			get
			{
				var parentStack = new Stack<RedBlackNode<T>>();

				var current = Root;

				while (parentStack.Count > 0 || current != null)
				{
					if (current != null)
					{
						yield return current;

						parentStack.Push(current.Right);
						current = current.Left;
					}
					else
					{
						current = parentStack.Pop();
					}
				}
			}
		}
		#endregion

		#region Value Iterators
		/// <summary>
		///     Iterates through the tree in pre order. Visiting the root, then the left sub-tree, then the right sub-tree. This is
		///     useful for algorithms such as the iterative height calculation as it starts at the top and works its way down
		///     through the levels
		/// </summary>
		public IEnumerable<T> PreOrderIterator
		{
			get
			{
				foreach (var node in PreOrderNodeIterator)
				{
					yield return node.Value;
				}
			}
		}

		/// <summary>
		///     Iterates through the tree in order. Visiting the left sub-tree, then the root, then the right sub-tree so that the
		///     values are returned in order.
		/// </summary>
		public IEnumerable<T> InOrderIterator
		{
			get
			{
				foreach (var node in InOrderNodeIterator)
				{
					yield return node.Value;
				}
			}
		}

		/// <summary>
		///     Iterates through the tree in post order. Visiting the left sub-tree, then the right sub-tree, then the root. This
		///     is useful if you need to dispose of the entire tree as items will be disposed from the bottom up
		/// </summary>
		public IEnumerable<T> PostOrderIterator
		{
			get
			{
				foreach (var node in PostOrderNodeIterator)
				{
					yield return node.Value;
				}
			}
		}
		#endregion
	}
}