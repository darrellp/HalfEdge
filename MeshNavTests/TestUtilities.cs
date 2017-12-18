using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;

namespace MeshNavTests
{
	[TestClass]
	public class TestUtilities
	{
		[TestMethod]
		public void TestBuilders()
		{
			var vdbl = Utilities.DblBuilder.DenseOfArray(new double[] {3, 4, 5});
			Assert.AreEqual(3.0, vdbl[0]);
			Assert.AreEqual(4.0, vdbl[1]);
			Assert.AreEqual(5.0, vdbl[2]);
			var vflt = Utilities.FloatBuilder.DenseOfArray(new float[] { 3, 4, 5 });
			Assert.AreEqual(3.0f, vflt[0]);
			Assert.AreEqual(4.0f, vflt[1]);
			Assert.AreEqual(5.0f, vflt[2]);
		}

		[TestMethod]
		public void TestToDouble()
		{
			var vdbl = Utilities.DblBuilder.DenseOfArray(new double[] { 3, 4, 5 });
			var vflt = Utilities.FloatBuilder.DenseOfArray(new float[] { 3, 4, 5 });
			var vdbldbl = vdbl.ToDouble();
			Assert.AreEqual(3.0, vdbldbl[0]);
			Assert.AreEqual(4.0, vdbldbl[1]);
			Assert.AreEqual(5.0, vdbldbl[2]);
			var vfltdbl = vflt.ToDouble();
			Assert.AreEqual(3.0, vfltdbl[0]);
			Assert.AreEqual(4.0, vfltdbl[1]);
			Assert.AreEqual(5.0, vfltdbl[2]);
		}

		[TestMethod]
		public void TestCrossProduct()
		{
			var vdblx = Utilities.DblBuilder.DenseOfArray(new double[] { 1, 0, 0 });
			var vdbly = Utilities.DblBuilder.DenseOfArray(new double[] { 0, 1, 0 });
			var xpDbl = Utilities.CrossProduct(vdblx, vdbly);
			Assert.AreEqual(0.0, xpDbl[0]);
			Assert.AreEqual(0.0, xpDbl[1]);
			Assert.AreEqual(1.0, xpDbl[2]);

			var vfltx = Utilities.FloatBuilder.DenseOfArray(new float[] { 1, 0, 0 });
			var vflty = Utilities.FloatBuilder.DenseOfArray(new float[] { 0, 1, 0 });
			var xpflt = Utilities.CrossProduct(vfltx, vflty);
			Assert.AreEqual(0.0, xpflt[0]);
			Assert.AreEqual(0.0, xpflt[1]);
			Assert.AreEqual(1.0, xpflt[2]);
		}

		[TestMethod]
		public void TestScalarArithmetic()
		{
			var vdbl = Utilities.DblBuilder.DenseOfArray(new double[] { 2, 4, 6 });
			var vdblHalf = vdbl.ScalarDivide(2);
			Assert.AreEqual(1.0, vdblHalf[0]);
			Assert.AreEqual(2.0, vdblHalf[1]);
			Assert.AreEqual(3.0, vdblHalf[2]);
			var vdblDouble = vdblHalf.ScalarMultiply(2);
			Assert.AreEqual(vdbl[0], vdblDouble[0]);
			Assert.AreEqual(vdbl[1], vdblDouble[1]);
			Assert.AreEqual(vdbl[2], vdblDouble[2]);

			var vflt = Utilities.FloatBuilder.DenseOfArray(new float[] { 2, 4, 6 });
			var vfltHalf = vflt.ScalarDivide(2);
			Assert.AreEqual(1.0, vfltHalf[0]);
			Assert.AreEqual(2.0, vfltHalf[1]);
			Assert.AreEqual(3.0, vfltHalf[2]);
			var vfltDouble = vfltHalf.ScalarMultiply(2);
			Assert.AreEqual(vflt[0], vfltDouble[0]);
			Assert.AreEqual(vflt[1], vfltDouble[1]);
			Assert.AreEqual(vflt[2], vfltDouble[2]);

		}
	}
}