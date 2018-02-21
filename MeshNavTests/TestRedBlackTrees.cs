using Common.Collections.Generic;
using MeshNav;
using MeshNav.RedBlack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable RedundantArgumentDefaultValue

namespace MeshNavTests
{
	[TestClass]
	public class TestRedBlackTree
	{
		public TestContext TestContext { set; get; }

		#region Setup
		public RedBlackTree<int> Empty;
		public RedBlackTree<int> RootOnly;
		public RedBlackTree<int> RootLeft;
		public RedBlackTree<int> RootRight;
		public RedBlackTree<int> ThreeNodesFull;
		public RedBlackTree<int> FourNodesLeftLeft;
		public RedBlackTree<int> FourNodesLeftRight;
		public RedBlackTree<int> FourNodesRightLeft;
		public RedBlackTree<int> FourNodesRightRight;
		public RedBlackTree<int> FiveNodesLeftFull;
		public RedBlackTree<int> Bigger;


		public RedBlackTree<int> InstanceEmpty => Empty;
		public RedBlackTree<int> InstanceRootOnly => RootOnly;
		public RedBlackTree<int> InstanceRootLeft => RootLeft;
		public RedBlackTree<int> InstanceRootRight => RootRight;
		public RedBlackTree<int> InstanceThreeNodesFull => ThreeNodesFull;
		public RedBlackTree<int> InstanceFourNodesLeftLeft => FourNodesLeftLeft;
		public RedBlackTree<int> InstanceFourNodesLeftRight => FourNodesLeftRight;
		public RedBlackTree<int> InstanceFourNodesRightLeft => FourNodesRightLeft;
		public RedBlackTree<int> InstanceFourNodesRightRight => FourNodesRightRight;
		public RedBlackTree<int> InstanceFiveNodesLeftFull => FiveNodesLeftFull;
		public RedBlackTree<int> InstanceBigger => Bigger;
		public RedBlackTree<int> InstanceDeleteBlackRightLeafRedSibling1 { get; set; }
		public RedBlackTree<int> InstanceDeleteBlackLeftLeafRedSibling1 { get; set; }
		public RedBlackTree<int> InstanceDeleteBlackRightLeafRedSibling2 { get; set; }
		public RedBlackTree<int> InstanceDeleteBlackLeftLeafRedSibling2 { get; set; }

		[TestInitialize]
		public void InitTreesForTests()
		{
			if (TestContext.TestName.Contains("Empty"))
			{
				InitEmptyTreeForTests();
			}
			else if (TestContext.TestName.Contains("RootOnly"))
			{
				InitRootOnlyForTests();
			}
			else if (TestContext.TestName.Contains("RootLeft"))
			{
				InitRootLeftForTests();
			}
			else if (TestContext.TestName.Contains("RootRight"))
			{
				InitRootRightForTests();
			}
			else if (TestContext.TestName.Contains("ThreeNodesFull"))
			{
				InitThreeNodesFullForTests();
			}
			else if (TestContext.TestName.Contains("FourNodesLeftLeft"))
			{
				InitFourNodesLeftLeft();
			}
			else if (TestContext.TestName.Contains("FourNodesLeftRight"))
			{
				InitFourNodesLeftRight();
			}
			else if (TestContext.TestName.Contains("FourNodesRightLeft"))
			{
				InitFourNodesRightLeft();
			}
			else if (TestContext.TestName.Contains("FourNodesRightRight"))
			{
				InitFourNodesRightRight();
			}
			else if (TestContext.TestName.Contains("FiveNodesLeftFull"))
			{
				InitFiveNodesLeftFullForTests();
			}
			else if (TestContext.TestName.Contains("Bigger"))
			{
				InitBiggerForTests();
			}
			else
			{
				InitCustomForTests();
			}
		}

		public void InitEmptyTreeForTests()
		{
			Empty = new RedBlackTree<int>();
		}

		public void InitRootOnlyForTests()
		{
			RootOnly = new RedBlackTree<int>
			{
				Count = 1,
				Root = new RedBlackNode<int>(50, Color.Black)
			};
		}

		public void InitRootLeftForTests()
		{
			RootLeft = new RedBlackTree<int>
			{
				Count = 2,
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Red)
				}
			};
		}

		public void InitRootRightForTests()
		{
			RootRight = new RedBlackTree<int>
			{
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Right = new RedBlackNode<int>(75, Color.Red)
				},
				Count = 2
			};
		}

		public void InitThreeNodesFullForTests()
		{
			ThreeNodesFull = new RedBlackTree<int>
			{
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Red),
					Right = new RedBlackNode<int>(75, Color.Red)
				},
				Count = 3
			};
		}

		public void InitFourNodesLeftLeft()
		{
			FourNodesLeftLeft = new RedBlackTree<int>
			{
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Black)
					{
						Left = new RedBlackNode<int>(12, Color.Red)
					},
					Right = new RedBlackNode<int>(75, Color.Black)
				},
				Count = 4
			};
		}

		public void InitFourNodesLeftRight()
		{
			FourNodesLeftRight = new RedBlackTree<int>
			{
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Black)
					{
						Right = new RedBlackNode<int>(32, Color.Red)
					},
					Right = new RedBlackNode<int>(75, Color.Black)
				},
				Count = 4
			};
		}

		public void InitFourNodesRightLeft()
		{
			FourNodesRightLeft = new RedBlackTree<int>
			{
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Black),
					Right = new RedBlackNode<int>(75, Color.Black)
					{
						Left = new RedBlackNode<int>(63, Color.Red)
					}
				},
				Count = 4
			};
		}

		public void InitFourNodesRightRight()
		{
			FourNodesRightRight = new RedBlackTree<int>
			{
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Black),
					Right = new RedBlackNode<int>(75, Color.Black)
					{
						Right = new RedBlackNode<int>(100, Color.Red)
					}
				},
				Count = 4
			};
		}


		public void InitFiveNodesLeftFullForTests()
		{
			FiveNodesLeftFull = new RedBlackTree<int>
			{
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Black)
					{
						Left = new RedBlackNode<int>(12, Color.Red),
						Right = new RedBlackNode<int>(32, Color.Red)
					},
					Right = new RedBlackNode<int>(75, Color.Black)
				},
				Count = 5
			};
		}

		public void InitBiggerForTests()
		{
			Bigger = new RedBlackTree<int>
			{
				Count = 9,
				Root = new RedBlackNode<int>(100, Color.Black)
				{
					Left = new RedBlackNode<int>(50, Color.Red)
					{
						Left = new RedBlackNode<int>(25, Color.Black)
						{
							Right = new RedBlackNode<int>(30, Color.Red)
						},
						Right = new RedBlackNode<int>(75, Color.Black)
					},
					Right = new RedBlackNode<int>(150, Color.Red)
					{
						Left = new RedBlackNode<int>(125, Color.Black),
						Right = new RedBlackNode<int>(175, Color.Black)
						{
							Left = new RedBlackNode<int>(160, Color.Red)
						}
					}
				}
			};
		}

		public void InitDelete_BlackLeftLeafRedSibling1()
		{
			InstanceDeleteBlackLeftLeafRedSibling1 = new RedBlackTree<int>
			{
				Count = 0,
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Black),
					Right = new RedBlackNode<int>(75, Color.Red)
					{
						Left = new RedBlackNode<int>(63, Color.Black),
						Right = new RedBlackNode<int>(100, Color.Black)
						{
							Left = new RedBlackNode<int>(80, Color.Red)
						}
					}
				}
			};
		}

		public void InitDelete_BlackRightLeafRedSibling1()
		{
			InstanceDeleteBlackRightLeafRedSibling1 = new RedBlackTree<int>
			{
				Count = 0,
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Red)
					{
						Left = new RedBlackNode<int>(12, Color.Black)
						{
							Right = new RedBlackNode<int>(20, Color.Red)
						},
						Right = new RedBlackNode<int>(32, Color.Black)
					},
					Right = new RedBlackNode<int>(75, Color.Black)
				}
			};
		}

		public void InitDelete_BlackLeftLeafRedSibling2()
		{
			InstanceDeleteBlackLeftLeafRedSibling2 = new RedBlackTree<int>
			{
				Count = 0,
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Black),
					Right = new RedBlackNode<int>(75, Color.Red)
					{
						Left = new RedBlackNode<int>(60, Color.Black)
						{
							Right = new RedBlackNode<int>(70, Color.Red)
						},
						Right = new RedBlackNode<int>(100, Color.Black)
					}
				}
			};
		}

		public void InitDelete_BlackRightLeafRedSibling2()
		{
			InstanceDeleteBlackRightLeafRedSibling2 = new RedBlackTree<int>
			{
				Count = 0,
				Root = new RedBlackNode<int>(50, Color.Black)
				{
					Left = new RedBlackNode<int>(25, Color.Red)
					{
						Left = new RedBlackNode<int>(10, Color.Black),
						Right = new RedBlackNode<int>(40, Color.Black)
						{
							Left = new RedBlackNode<int>(30, Color.Red)
						}
					},
					Right = new RedBlackNode<int>(75, Color.Black)
				}
			};
		}

		public void InitCustomForTests()
		{
			switch (TestContext.TestName)
			{
				case "Delete_BlackLeftLeafRedSibling1":
					InitDelete_BlackLeftLeafRedSibling1();
					break;
				case "Delete_BlackRightLeafRedSibling1":
					InitDelete_BlackRightLeafRedSibling1();
					break;
				case "Delete_BlackLeftLeafRedSibling2":
					InitDelete_BlackLeftLeafRedSibling2();
					break;
				case "Delete_BlackRightLeafRedSibling2":
					InitDelete_BlackRightLeafRedSibling2();
					break;
			}
		}

		public void CleanupCustom()
		{
			InstanceDeleteBlackLeftLeafRedSibling1 = null;
			InstanceDeleteBlackLeftLeafRedSibling2 = null;
			InstanceDeleteBlackRightLeafRedSibling1 = null;
			InstanceDeleteBlackRightLeafRedSibling2 = null;
		}
		#endregion

		#region RedBlackTree.Insert
		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Insert_ThreeNodesFull_RightRight()
		{
			InstanceThreeNodesFull.Insert(100);

			Assert.AreEqual(50, InstanceThreeNodesFull.Root.Value);
			Assert.AreEqual(25, InstanceThreeNodesFull.Root.Left.Value);
			Assert.AreEqual(75, InstanceThreeNodesFull.Root.Right.Value);
			Assert.AreEqual(100, InstanceThreeNodesFull.Root.Right.Right.Value);

			Assert.IsFalse(InstanceThreeNodesFull.Root.IsRed);
			Assert.IsFalse(InstanceThreeNodesFull.Root.Left.IsRed);
			Assert.IsFalse(InstanceThreeNodesFull.Root.Right.IsRed);
			Assert.IsTrue(InstanceThreeNodesFull.Root.Right.Right.IsRed);

			Assert.IsNull(InstanceThreeNodesFull.Root.Left.Left);
			Assert.IsNull(InstanceThreeNodesFull.Root.Left.Right);
			Assert.IsNull(InstanceThreeNodesFull.Root.Right.Left);
			Assert.IsNull(InstanceThreeNodesFull.Root.Right.Right.Left);
			Assert.IsNull(InstanceThreeNodesFull.Root.Right.Right.Right);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Insert_ThreeNodesFull_LeftLeft()
		{
			ThreeNodesFull.Insert(1);

			Assert.AreEqual(50, InstanceThreeNodesFull.Root.Value);
			Assert.AreEqual(25, InstanceThreeNodesFull.Root.Left.Value);
			Assert.AreEqual(75, InstanceThreeNodesFull.Root.Right.Value);
			Assert.AreEqual(1, InstanceThreeNodesFull.Root.Left.Left.Value);

			Assert.IsFalse(InstanceThreeNodesFull.Root.IsRed);
			Assert.IsFalse(InstanceThreeNodesFull.Root.Left.IsRed);
			Assert.IsFalse(InstanceThreeNodesFull.Root.Right.IsRed);
			Assert.IsTrue(InstanceThreeNodesFull.Root.Left.Left.IsRed);

			Assert.IsNull(InstanceThreeNodesFull.Root.Left.Left.Left);
			Assert.IsNull(InstanceThreeNodesFull.Root.Left.Left.Right);
			Assert.IsNull(InstanceThreeNodesFull.Root.Left.Right);
			Assert.IsNull(InstanceThreeNodesFull.Root.Right.Left);
			Assert.IsNull(InstanceThreeNodesFull.Root.Right.Right);
		}
		#endregion

		#region RedBlackTree.Delete
		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_FourNodesLeftLeft_BlackLeaf()
		{
			Assert.IsTrue(FourNodesLeftLeft.Delete(75));

			Assert.AreEqual(25, InstanceFourNodesLeftLeft.Root.Value);
			Assert.AreEqual(12, InstanceFourNodesLeftLeft.Root.Left.Value);
			Assert.AreEqual(50, InstanceFourNodesLeftLeft.Root.Right.Value);

			Assert.IsFalse(InstanceFourNodesLeftLeft.Root.IsRed);
			Assert.IsFalse(InstanceFourNodesLeftLeft.Root.Left.IsRed);
			Assert.IsFalse(InstanceFourNodesLeftLeft.Root.Right.IsRed);

			Assert.IsNull(InstanceFourNodesLeftLeft.Root.Left.Left);
			Assert.IsNull(InstanceFourNodesLeftLeft.Root.Left.Right);
			Assert.IsNull(InstanceFourNodesLeftLeft.Root.Right.Left);
			Assert.IsNull(InstanceFourNodesLeftLeft.Root.Right.Right);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_FourNodesRightRight_BlackLeaf()
		{
			Assert.IsTrue(FourNodesRightRight.Delete(25));

			Assert.AreEqual(75, InstanceFourNodesRightRight.Root.Value);
			Assert.AreEqual(50, InstanceFourNodesRightRight.Root.Left.Value);
			Assert.AreEqual(100, InstanceFourNodesRightRight.Root.Right.Value);

			Assert.IsFalse(InstanceFourNodesRightRight.Root.IsRed);
			Assert.IsFalse(InstanceFourNodesRightRight.Root.Left.IsRed);
			Assert.IsFalse(InstanceFourNodesRightRight.Root.Right.IsRed);

			Assert.IsNull(InstanceFourNodesRightRight.Root.Left.Left);
			Assert.IsNull(InstanceFourNodesRightRight.Root.Left.Right);
			Assert.IsNull(InstanceFourNodesRightRight.Root.Right.Left);
			Assert.IsNull(InstanceFourNodesRightRight.Root.Right.Right);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_BlackLeftLeafRedSibling1()
		{
			Assert.IsTrue(InstanceDeleteBlackLeftLeafRedSibling1.Delete(25));

			Assert.AreEqual(75, InstanceDeleteBlackLeftLeafRedSibling1.Root.Value);
			Assert.AreEqual(50, InstanceDeleteBlackLeftLeafRedSibling1.Root.Left.Value);
			Assert.AreEqual(100, InstanceDeleteBlackLeftLeafRedSibling1.Root.Right.Value);
			Assert.AreEqual(63, InstanceDeleteBlackLeftLeafRedSibling1.Root.Left.Right.Value);
			Assert.AreEqual(80, InstanceDeleteBlackLeftLeafRedSibling1.Root.Right.Left.Value);

			Assert.IsFalse(InstanceDeleteBlackLeftLeafRedSibling1.Root.IsRed);
			Assert.IsFalse(InstanceDeleteBlackLeftLeafRedSibling1.Root.Left.IsRed);
			Assert.IsFalse(InstanceDeleteBlackLeftLeafRedSibling1.Root.Right.IsRed);
			Assert.IsTrue(InstanceDeleteBlackLeftLeafRedSibling1.Root.Left.Right.IsRed);
			Assert.IsTrue(InstanceDeleteBlackLeftLeafRedSibling1.Root.Right.Left.IsRed);

			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling1.Root.Left.Left);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling1.Root.Left.Right.Left);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling1.Root.Left.Right.Right);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling1.Root.Right.Right);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling1.Root.Right.Left.Left);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling1.Root.Right.Left.Right);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_BlackRightLeafRedSibling1()
		{
			Assert.IsTrue(InstanceDeleteBlackRightLeafRedSibling1.Delete(75));

			Assert.AreEqual(25, InstanceDeleteBlackRightLeafRedSibling1.Root.Value);
			Assert.AreEqual(12, InstanceDeleteBlackRightLeafRedSibling1.Root.Left.Value);
			Assert.AreEqual(50, InstanceDeleteBlackRightLeafRedSibling1.Root.Right.Value);
			Assert.AreEqual(20, InstanceDeleteBlackRightLeafRedSibling1.Root.Left.Right.Value);
			Assert.AreEqual(32, InstanceDeleteBlackRightLeafRedSibling1.Root.Right.Left.Value);

			Assert.IsFalse(InstanceDeleteBlackRightLeafRedSibling1.Root.IsRed);
			Assert.IsFalse(InstanceDeleteBlackRightLeafRedSibling1.Root.Left.IsRed);
			Assert.IsFalse(InstanceDeleteBlackRightLeafRedSibling1.Root.Right.IsRed);
			Assert.IsTrue(InstanceDeleteBlackRightLeafRedSibling1.Root.Left.Right.IsRed);
			Assert.IsTrue(InstanceDeleteBlackRightLeafRedSibling1.Root.Right.Left.IsRed);

			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling1.Root.Left.Left);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling1.Root.Left.Right.Left);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling1.Root.Left.Right.Right);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling1.Root.Right.Right);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling1.Root.Right.Left.Left);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling1.Root.Right.Left.Right);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_BlackLeftLeafRedSibling2()
		{
			Assert.IsTrue(InstanceDeleteBlackLeftLeafRedSibling2.Delete(25));

			Assert.AreEqual(75, InstanceDeleteBlackLeftLeafRedSibling2.Root.Value);
			Assert.AreEqual(60, InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Value);
			Assert.AreEqual(100, InstanceDeleteBlackLeftLeafRedSibling2.Root.Right.Value);
			Assert.AreEqual(50, InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Left.Value);
			Assert.AreEqual(70, InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Right.Value);

			Assert.IsFalse(InstanceDeleteBlackLeftLeafRedSibling2.Root.IsRed);
			Assert.IsTrue(InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.IsRed);
			Assert.IsFalse(InstanceDeleteBlackLeftLeafRedSibling2.Root.Right.IsRed);
			Assert.IsFalse(InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Left.IsRed);
			Assert.IsFalse(InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Right.IsRed);

			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Left.Left);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Left.Right);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Right.Left);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling2.Root.Left.Right.Right);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling2.Root.Right.Left);
			Assert.IsNull(InstanceDeleteBlackLeftLeafRedSibling2.Root.Right.Right);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_BlackRightLeafRedSibling2()
		{
			Assert.IsTrue(InstanceDeleteBlackRightLeafRedSibling2.Delete(75));

			Assert.AreEqual(25, InstanceDeleteBlackRightLeafRedSibling2.Root.Value);
			Assert.AreEqual(10, InstanceDeleteBlackRightLeafRedSibling2.Root.Left.Value);
			Assert.AreEqual(40, InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Value);
			Assert.AreEqual(30, InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Left.Value);
			Assert.AreEqual(50, InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Right.Value);

			Assert.IsFalse(InstanceDeleteBlackRightLeafRedSibling2.Root.IsRed);
			Assert.IsFalse(InstanceDeleteBlackRightLeafRedSibling2.Root.Left.IsRed);
			Assert.IsTrue(InstanceDeleteBlackRightLeafRedSibling2.Root.Right.IsRed);
			Assert.IsFalse(InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Left.IsRed);
			Assert.IsFalse(InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Right.IsRed);

			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Left.Left);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Left.Right);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Right.Left);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling2.Root.Right.Right.Right);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling2.Root.Left.Left);
			Assert.IsNull(InstanceDeleteBlackRightLeafRedSibling2.Root.Left.Right);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_FourNodesLeftRight_BlackLeaf()
		{
			Assert.IsTrue(FourNodesLeftRight.Delete(75));

			Assert.AreEqual(32, InstanceFourNodesLeftRight.Root.Value);
			Assert.AreEqual(25, InstanceFourNodesLeftRight.Root.Left.Value);
			Assert.AreEqual(50, InstanceFourNodesLeftRight.Root.Right.Value);

			Assert.IsFalse(InstanceFourNodesLeftRight.Root.IsRed);
			Assert.IsFalse(InstanceFourNodesLeftRight.Root.Left.IsRed);
			Assert.IsFalse(InstanceFourNodesLeftRight.Root.Right.IsRed);

			Assert.IsNull(InstanceFourNodesLeftRight.Root.Left.Left);
			Assert.IsNull(InstanceFourNodesLeftRight.Root.Left.Right);
			Assert.IsNull(InstanceFourNodesLeftRight.Root.Right.Left);
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		public void Delete_FourNodesRightLeft_BlackLeaf()
		{
			Assert.IsTrue(FourNodesRightLeft.Delete(25));

			Assert.AreEqual(63, InstanceFourNodesRightLeft.Root.Value);
			Assert.AreEqual(50, InstanceFourNodesRightLeft.Root.Left.Value);
			Assert.AreEqual(75, InstanceFourNodesRightLeft.Root.Right.Value);

			Assert.IsFalse(InstanceFourNodesRightLeft.Root.IsRed);
			Assert.IsFalse(InstanceFourNodesRightLeft.Root.Left.IsRed);
			Assert.IsFalse(InstanceFourNodesRightLeft.Root.Right.IsRed);

			Assert.IsNull(InstanceFourNodesRightLeft.Root.Left.Left);
			Assert.IsNull(InstanceFourNodesRightLeft.Root.Left.Right);
			Assert.IsNull(InstanceFourNodesRightLeft.Root.Right.Left);
		}
		#endregion

		#region RedBlackTree.AssertTree
		[TestMethod]
		[TestCategory("RedBlackTree")]
		[ExpectedException(typeof(MeshNavException))]
		public void AssertValidTree_InvalidChildren()
		{
			var root = new RedBlackNode<int>(100) {Left = new RedBlackNode<int>(150)};

			var bst = new RedBlackTree<int> {Root = root};

			bst.AssertValidTree();
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		[ExpectedException(typeof(MeshNavException))]
		public void AssertValidTree_Invalid_DoubleRed_Left()
		{
			var root = new RedBlackNode<int>(100, Color.Black)
			{
				Left = new RedBlackNode<int>(50, Color.Red),
				Right = new RedBlackNode<int>(150, Color.Red)
			};
			root.Left.Left = new RedBlackNode<int>(40, Color.Red);

			var bst = new RedBlackTree<int> {Root = root};

			bst.AssertValidTree();
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		[ExpectedException(typeof(MeshNavException))]
		public void AssertValidTree_Invalid_DoubleRed_Right()
		{
			var root = new RedBlackNode<int>(100, Color.Black)
			{
				Left = new RedBlackNode<int>(50, Color.Red),
				Right = new RedBlackNode<int>(150, Color.Red) {Right = new RedBlackNode<int>(160, Color.Red)}
			};

			var bst = new RedBlackTree<int> {Root = root};

			bst.AssertValidTree();
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		[ExpectedException(typeof(MeshNavException))]
		public void AssertValidTree_Invalid_BlackMismatch_Left()
		{
			var root = new RedBlackNode<int>(100, Color.Black)
			{
				Left = new RedBlackNode<int>(50, Color.Red),
				Right = new RedBlackNode<int>(150, Color.Red)
			};
			root.Left.Left = new RedBlackNode<int>(40, Color.Red);

			var bst = new RedBlackTree<int> {Root = root};

			bst.AssertValidTree();
		}

		[TestMethod]
		[TestCategory("RedBlackTree")]
		[ExpectedException(typeof(MeshNavException))]
		public void AssertValidTree_Invalid_BlackMismatch_Right()
		{
			var root = new RedBlackNode<int>(100, Color.Black)
			{
				Left = new RedBlackNode<int>(50, Color.Red),
				Right = new RedBlackNode<int>(150, Color.Red) {Right = new RedBlackNode<int>(160, Color.Red)}
			};

			var bst = new RedBlackTree<int> {Root = root};

			bst.AssertValidTree();
		}
		#endregion
	}
}