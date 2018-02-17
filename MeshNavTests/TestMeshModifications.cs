using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using MeshNav.BoundaryMesh;
using Templates;

namespace MeshNavTests
{
    /// <summary>
    /// Summary description for TestMeshModifications
    /// </summary>
    [TestClass]
    public class TestMeshModifications
    {
        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        private Face BuildCcwSquare(Mesh mesh)
        {
            // ReSharper disable InconsistentNaming
            var ptLL = mesh.AddVertex(0, 0);
            var ptLR = mesh.AddVertex(1, 0);
            var ptUL = mesh.AddVertex(0, 1);
            var ptUR = mesh.AddVertex(1, 1);
            // ReSharper restore InconsistentNaming
            return mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
        }

        private Face BuildCwSquare(Mesh mesh)
        {
            // ReSharper disable InconsistentNaming
            var ptLL = mesh.AddVertex(3, 3);
            var ptLR = mesh.AddVertex(4, 3);
            var ptUL = mesh.AddVertex(3, 4);
            var ptUR = mesh.AddVertex(4, 4);
            // ReSharper restore InconsistentNaming
            return mesh.AddFace(ptLL, ptUL, ptUR, ptLR);
        }

        [TestMethod]
        public void TestFaceCcw()
        {
            var mesh = new BndFactory(2).CreateMesh() as BndMesh;

            var face = BuildCcwSquare(mesh);
            // ReSharper disable once PossibleNullReferenceException
            mesh.FinalizeMesh();

            Assert.IsTrue(face.IsCcw);
        }

	    [TestMethod]
	    public void TestSetOrientation()
	    {
		    var mesh = new BndFactory(2).CreateMesh() as BndMesh;

		    var face = BuildCcwSquare(mesh);
	        // ReSharper disable PossibleNullReferenceException
		    mesh.FinalizeMesh();
		    var vertsCcw = face.Vertices().ToList();
	        var boundaryFace = mesh.Faces.First(f => f != face);
	        Assert.IsTrue(boundaryFace.IsBoundary);
            Assert.IsFalse(face.IsBoundary);
		    Assert.IsTrue(face.Vertices().Zip(vertsCcw, (v1, v2) => v1 == v2).All(f => f));
			mesh.SetOrientation(true);
		    Assert.IsTrue(face.Vertices().Zip(vertsCcw, (v1, v2) => v1 == v2).All(f => f));
			mesh.SetOrientation(false);
            // We are reversed, but in flipping the first edge to point (0,0)->(0,1) so that it
            // points (0,1)->(0,0) the initial vertex of the face has been changed to (0,1) so
            // we have to shift to account for that.
	        vertsCcw = vertsCcw.Skip(2).Concat(vertsCcw.Take(2)).ToList();
	        // ReSharper restore PossibleNullReferenceException

		    Assert.IsTrue(face.Vertices().Reverse().Zip(vertsCcw, (v1, v2) => v1 == v2).All(f => f));
		}

        [TestMethod]
        public void TestSetOrientationTwoBnds()
        {
            var mesh = new BndFactory(2).CreateMesh() as BndMesh;

            var faceCcw = BuildCcwSquare(mesh) as BoundaryFace;
            var faceCw = BuildCwSquare(mesh) as BoundaryFace;

            // ReSharper disable PossibleNullReferenceException
            mesh.FinalizeMesh();
            var vertsCcw = faceCcw.Vertices().ToList();
            var vertsCw = faceCw.Vertices().ToList();
            // This is the outer face surrounding faceCcw
            var boundaryFaceCw = mesh.Faces.First(f => f.IsBoundary &&  f.IsCcw);
            // This is the outer face surrounding faceCw
            var boundaryFaceCcw = mesh.Faces.First(f => f.IsBoundary && f.IsCcw);

            Assert.IsFalse(faceCcw.IsBoundary);
            Assert.IsFalse(faceCw.IsBoundary);

            Assert.IsTrue(faceCcw.Vertices().Zip(vertsCcw, (v1, v2) => v1 == v2).All(f => f));
            Assert.IsTrue(faceCw.Vertices().Zip(vertsCw, (v1, v2) => v1 == v2).All(f => f));
            mesh.SetOrientation(true);
            Assert.IsTrue(faceCcw.Vertices().Zip(vertsCcw, (v1, v2) => v1 == v2).All(f => f));
            vertsCw = vertsCw.Skip(2).Concat(vertsCw.Take(2)).ToList();
            Assert.IsTrue(faceCw.Vertices().Reverse().Zip(vertsCw, (v1, v2) => v1 == v2).All(f => f));
        }
    }
}
