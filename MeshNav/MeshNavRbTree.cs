using System;
using System.Collections.Generic;
using System.Linq;
using Common.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MeshNav.RedBlack;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNav
{
	class MeshNavRbTree : RedBlackTree<RbType>
	{
		internal (RbType higher, RbType lower) InsertBracketed(RbType val)
		{
			RbType higher;
			RbType lower;

			(Root, higher, lower) = InsertBracketed(Root, new RedBlackNode<RbType>(val));

			if (Root.IsRed)
			{
				Root.SetColor(Color.Black);
			}

			return (higher, lower);
		}

		/// <summary>
		///     Inserts the given node underneath the given root according to the BinarySearchTree algorithm and then
		///     rebalances the tree according to the red-black tree rules
		/// </summary>
		/// <param name="root">The root node of the tree</param>
		/// <param name="node">The node to insert</param>
		/// <returns>The new root of the tree as it may have changed</returns>
		private (RedBlackNode<RbType> root, RbType higher, RbType lower) InsertBracketed(RedBlackNode<RbType> root, RedBlackNode<RbType> node)
		{
			RbType higher = null;
			RbType lower = null;

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
					(root.Left, higher, lower) = InsertBracketed(root.Left, node);
					if (higher == null)
					{
						higher = root.Value;
						lower = root.Value.NextLower;
						higher.NextLower = node.Value;
						if (lower != null)
						{
							lower.NextHigher = node.Value;
						}
					}
				}
				else if (compareResult < 0)
				{
					(root.Right, higher, lower) = InsertBracketed(root.Right, node);
					if (lower == null)
					{
						lower = root.Value;
						higher = root.Value.NextHigher;
						lower.NextHigher = node.Value;
						if (higher != null)
						{
							higher.NextLower = node.Value;
						}
					}
				}
				else
				{
					throw new ArgumentException("Inserting duplicate in Red Black tree");
				}
			}

			root = PostInsertCleanup(root, node);

			return (root, higher, lower);
		}
	}
}
