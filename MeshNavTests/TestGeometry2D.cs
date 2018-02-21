using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void TestSignedArea()
        {
            var poly = new List<Vector>()
            {
                new Vector(1, 0),
                new Vector(0, 0),
                new Vector(0, 1),
                new Vector(1, 1)
            };
            Assert.AreEqual(-1, SignedArea(poly));
        }

        [TestMethod]
        public void TestVector()
        {
            var v = new Vector(1, 2);
            Assert.AreEqual(v[0], 1.0f);
			Assert.AreEqual(v[1], 2.0f);
            Assert.AreEqual(v.X, 1.0);
            Assert.AreEqual(v.Y, 2.0);
        }

	    public void TestCall(Vector vec)
	    {
		    Assert.AreEqual(vec.X, 1.0);
	    }
		
        [TestMethod]
        public void TestPointInConvexPoly()
        {
            var poly = new List<Vector>()
                            {
                                new Vector(-1, -1),
                                new Vector(1, -1),
                                new Vector(1, 1),
                                new Vector(-1, 1)
                            };
            Assert.IsTrue(PointInConvexPoly(new Vector(0, 0), poly));
            Assert.IsFalse(PointInConvexPoly(new Vector(2, 0), poly));
            poly = new List<Vector>()
                    {
                        // ReSharper disable RedundantCast
                        new Vector(-1689, 9836),
                        new Vector(-6680, 7107),
                        new Vector((T)393.18, (T)37.905),
                        new Vector((T)394.025, (T)37.825),
                        new Vector(416, 59)
                        // ReSharper restore RedundantCast
                    };
            Assert.IsFalse(PointInConvexPoly(new Vector(423, 68), poly));
        }

        [TestMethod]
        public void TestCcw()
        {
            var pt1 = new Vector(1, 0);
            var pt2 = new Vector(0, 0);
            var pt3 = new Vector(1, 1);

            Assert.IsTrue(ICcw(pt1, pt2, pt3) < 0);
            Assert.IsTrue(ICcw(pt3, pt2, pt1) > 0);
        }

        [TestMethod]
        public void TestCircumcenter()
        {
            var pt1 = new Vector(0, 0);
            var pt2 = new Vector(1, 1);
            var pt3 = new Vector(1, -1);
            var pt4 = new Vector(2, 2);

            Assert.IsTrue(FFindCircumcenter(pt1, pt2, pt3, out var ptOut));
            Assert.IsTrue(FCloseEnough(ptOut.X, 1));
            Assert.IsTrue(Abs(ptOut.Y) <= Tolerance);
            Assert.IsFalse(FFindCircumcenter(pt1, pt2, pt4, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt2, pt1, pt1, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt1, pt2, pt1, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt1, pt1, pt2, out ptOut));
            Assert.IsFalse(FFindCircumcenter(pt1, pt1, pt1, out ptOut));
        }

        [TestMethod]
        public void TestParabolicCut()
        {
            var pt1 = new Vector(0, 0);
            var pt2 = new Vector(1, 1);
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt1, pt2, -1), -3));
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt2, pt1, -1), 1));

            pt1 = new Vector(0, 0);
            pt2 = new Vector(8, 4);
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt1, pt2, -1), -7));
            Assert.IsTrue(FCloseEnough(ParabolicCut(pt2, pt1, -1), 3));
        }
    }
    // ReSharper restore RedundantArgumentDefaultValue
}
