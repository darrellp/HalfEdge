using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra;
using MeshNav;
using static System.Math;
using static MeshNav.Geometry2D;
using static MeshNav.Utilities;
#if FLOAT
using T = System.Single;
#else
using T = System.Double;
#endif

namespace MeshNavTests
{
    // ReSharper disable RedundantArgumentDefaultValue
    [TestClass]
    public class TestG2D
    {
#pragma warning disable 1591
        [TestMethod]
        public void TestMake()
        {
            var v = Make(1, 2);
            Assert.AreEqual(v[0], 1.0f);
			Assert.AreEqual(v[1], 2.0f);
            Assert.AreEqual(v.X(), 1.0);
            Assert.AreEqual(v.Y(), 2.0);
        }

	    public void TestCall(Vector<T> vec)
	    {
		    Assert.AreEqual(vec.X(), 1.0);
	    }
		
        [TestMethod]
        public void TestPointInConvexPoly()
        {
            var poly = new List<Vector<T>>()
                            {
                                Make(-1, -1),
                                Make(1, -1),
                                Make(1, 1),
                                Make(-1, 1)
                            };
            Assert.IsTrue(PointInConvexPoly(Make(0, 0), poly));
            Assert.IsFalse(PointInConvexPoly(Make(2, 0), poly));
            poly = new List<Vector<T>>()
                    {
                        // ReSharper disable RedundantCast
                        Make(-1689, 9836),
                        Make(-6680, 7107),
                        Make((T)393.18, (T)37.905),
                        Make((T)394.025, (T)37.825),
                        Make(416, 59)
                        // ReSharper restore RedundantCast
                    };
            Assert.IsFalse(PointInConvexPoly(Make(423, 68), poly));
        }

        [TestMethod]
        public void TestCcw()
        {
            var pt1 = Make(1, 0);
            var pt2 = Make(0, 0);
            var pt3 = Make(1, 1);

            Assert.IsTrue(ICcw(pt1, pt2, pt3) < 0);
            Assert.IsTrue(ICcw(pt3, pt2, pt1) > 0);
        }

        [TestMethod]
        public void TestCircumcenter()
        {
            var pt1 = Make(0, 0);
            var pt2 = Make(1, 1);
            var pt3 = Make(1, -1);
            var pt4 = Make(2, 2);

            Assert.IsTrue(FFindCircumcenter(pt1, pt2, pt3, out var ptOut));
            Assert.IsTrue(FCloseEnough(ptOut.X(), 1));
            Assert.IsTrue(Abs(ptOut.Y()) <= Tolerance);
            Assert.IsFalse(FFindCircumcenter(pt1, pt2, pt4, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt2, pt1, pt1, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt1, pt2, pt1, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt1, pt1, pt2, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt1, pt1, pt1, out ptOut));
        }

        [TestMethod]
        public void TestParabolicCut()
        {
            var pt1 = Make(0, 0);
            var pt2 = Make(1, 1);
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt1, pt2, -1), -3));
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt2, pt1, -1), 1));

            pt1 = Make(0, 0);
            pt2 = Make(8, 4);
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt1, pt2, -1), -7));
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt2, pt1, -1), 3));
        }
    }
    // ReSharper restore RedundantArgumentDefaultValue
}
