using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra;
using MeshNav;
using static MeshNav.Utilities;
using GF = MeshNav.Geometry2D<float>;

namespace MeshNavTests
{
    [TestClass]
    public class TestG2D
    {
#pragma warning disable 1591
        [TestMethod]
        public void TestMake()
        {
            var v = Make<float>(1, 2);
	        //TestCall(v);
            Assert.AreEqual(v[0], 1.0f);
			Assert.AreEqual(v[1], 2.0f);
            Assert.AreEqual(v.XD(), 1.0);
            Assert.AreEqual(v.YD(), 2.0);
        }

	    public void TestCall<T>(Vector<T> vec) where T : struct, IEquatable<T>, IFormattable
	    {
		    Assert.AreEqual(vec.XD(), 1.0);
	    }
		
        [TestMethod]
        public void TestPointInConvexPoly()
        {
            List<Vector<float>> poly = new List<Vector<float>>()
                            {
                                Make<float>(-1, -1),
                                Make<float>(1, -1),
                                Make<float>(1, 1),
                                Make<float>(-1, 1)
                            };
            Assert.IsTrue(GF.PointInConvexPoly(Make<float>(0, 0), poly));
            Assert.IsFalse(GF.PointInConvexPoly(Make<float>(2, 0), poly));
            poly = new List<Vector<float>>()
                    {
                        Make<float>(-1689, 9836),
                        Make<float>(-6680, 7107),
                        Make<float>(393.18, 37.905),
                        Make<float>(394.025, 37.825),
                        Make<float>(416, 59)
                    };
            Assert.IsFalse(GF.PointInConvexPoly(Make<float>(423, 68), poly));
        }

        [TestMethod]
        public void TestCcw()
        {
            var pt1 = Make<float>(1, 0);
            var pt2 = Make<float>(0, 0);
            var pt3 = Make<float>(1, 1);

            Assert.IsTrue(GF.ICcw(pt1, pt2, pt3) < 0);
            Assert.IsTrue(GF.ICcw(pt3, pt2, pt1) > 0);
        }

        [TestMethod]
        public void TestCircumcenter()
        {
            var pt1 = Make<float>(0, 0);
            var pt2 = Make<float>(1, 1);
            var pt3 = Make<float>(1, -1);
            var pt4 = Make<float>(2, 2);

            Assert.IsTrue(GF.FFindCircumcenter(pt1, pt2, pt3, out var ptOut));
            Assert.IsTrue(GF.FCloseEnough(ptOut.XD(), 1));
            Assert.IsTrue(Math.Abs(ptOut.YD()) <= GF.Tolerance);
            Assert.IsFalse(GF.FFindCircumcenter(pt1, pt2, pt4, out ptOut));
            Assert.IsFalse(GF.FFindCircumcenter(pt2, pt1, pt1, out ptOut));
            Assert.IsFalse(GF.FFindCircumcenter(pt1, pt2, pt1, out ptOut));
            Assert.IsFalse(GF.FFindCircumcenter(pt1, pt1, pt2, out ptOut));
            Assert.IsFalse(GF.FFindCircumcenter(pt1, pt1, pt1, out ptOut));
        }

        [TestMethod]
        public void TestParabolicCut()
        {
            var pt1 = Make<float>(0, 0);
            var pt2 = Make<float>(1, 1);
            Assert.IsTrue(GF.FCloseEnough(GF.ParabolicCut(pt1, pt2, -1), -3));
            Assert.IsTrue(GF.FCloseEnough(GF.ParabolicCut(pt2, pt1, -1), 1));

            pt1 = Make<float>(0, 0);
            pt2 = Make<float>(8, 4);
            Assert.IsTrue(GF.FCloseEnough(GF.ParabolicCut(pt1, pt2, -1), -7));
            Assert.IsTrue(GF.FCloseEnough(GF.ParabolicCut(pt2, pt1, -1), 3));
        }
    }
}
