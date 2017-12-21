using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeshNav;
using MeshNav.BoundaryMesh;
using MeshNav.RayedMesh;

namespace MeshNavTests
{
    [TestClass]
    public class TestMesh
    {
        private Face<double> BuildSquare(Mesh<double> mesh)
        {
            // ReSharper disable InconsistentNaming
            var ptLL = mesh.AddVertex(0, 0);
            var ptLR = mesh.AddVertex(1, 0);
            var ptUL = mesh.AddVertex(0, 1);
            var ptUR = mesh.AddVertex(1, 1);
            // ReSharper restore InconsistentNaming
            return mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
        }

        [TestMethod]
        public void TestBuildSquare()
        {
            var mesh = new BoundaryMesh<double>(2);
            var face = BuildSquare(mesh);
            mesh.FinalizeMesh();

            Assert.AreEqual(4, face.Edges().Count());
            Assert.AreEqual(4, mesh.BoundaryFace.Edges().Count());
            foreach (var halfEdge in face.Edges())
            {
                Assert.AreEqual(face, halfEdge.Face);
                Assert.AreEqual(mesh.BoundaryFace, halfEdge.OppositeFace);
            }

            // Only real way to test the PreviousEdge is to debug and check that we use the
            // PreviousEdge stored in the HalfEdge rather than calculating by walking around
            // the face.
            var prevEdge = face.HalfEdge.PreviousEdge;
            Assert.AreEqual(prevEdge.NextVertex, face.HalfEdge.InitVertex);
        }

        [TestMethod]
        public void TestBuildAdjacentSquares()
        {
            var mesh = new BoundaryMesh<double>(2);

            var ptL0 = mesh.AddVertex(0, 0);
            var ptU0 = mesh.AddVertex(0, 1);
            var ptL1 = mesh.AddVertex(1, 0);
            var ptU1 = mesh.AddVertex(1, 1);
            var ptL2 = mesh.AddVertex(2, 0);
            var ptU2 = mesh.AddVertex(2, 1);

            var faceLeft = mesh.AddFace(ptL0, ptL1, ptU1, ptU0);
            var faceRight = mesh.AddFace(ptL1, ptL2, ptU2, ptU1);

            mesh.FinalizeMesh();
            Assert.AreEqual(4, faceLeft.Edges().Count());
            Assert.AreEqual(4, faceRight.Edges().Count());
            Assert.AreEqual(14, mesh.HalfEdges.Count());
            Assert.AreEqual(6, mesh.BoundaryFace.Edges().Count());
        }

        [TestMethod]
        public void TestWrongOrdering()
        {
            var mesh = new BoundaryMesh<double>(2);

            var ptL0 = mesh.AddVertex(0, 0);
            var ptU0 = mesh.AddVertex(0, 1);
            var ptL1 = mesh.AddVertex(1, 0);
            var ptU1 = mesh.AddVertex(1, 1);
            var ptL2 = mesh.AddVertex(2, 0);
            var ptU2 = mesh.AddVertex(2, 1);

            mesh.AddFace(ptL0, ptL1, ptU1, ptU0);
            var failed = false;
            try
            {
                // This vertex order is inconsistent with the first face added and should cause a failure
                mesh.AddFace(ptL1, ptU1, ptU2, ptL2);
            }
            catch (MeshNavException)
            {
                failed = true;
            }
            Assert.IsTrue(failed);
        }

        [TestMethod]
        public void TestCube()
        {
            var mesh = new BoundaryMesh<double>(3);

            var pt000 = mesh.AddVertex(0, 0, 0);
            var pt001 = mesh.AddVertex(0, 0, 1);
            var pt010 = mesh.AddVertex(0, 1, 0);
            var pt011 = mesh.AddVertex(0, 1, 1);
            var pt100 = mesh.AddVertex(1, 0, 0);
            var pt101 = mesh.AddVertex(1, 0, 1);
            var pt110 = mesh.AddVertex(1, 1, 0);
            var pt111 = mesh.AddVertex(1, 1, 1);

            var bottom = mesh.AddFace(pt000, pt100, pt110, pt010);
            var left = mesh.AddFace(pt000, pt010, pt011, pt001);
            var front = mesh.AddFace(pt000, pt001, pt101, pt100);
            var right = mesh.AddFace(pt100, pt101, pt111, pt110);
            var back = mesh.AddFace(pt010, pt110, pt111, pt011);
            var top = mesh.AddFace(pt001, pt011, pt111, pt101);

            mesh.FinalizeMesh();

            Assert.AreEqual(4, bottom.Edges().Count());
            Assert.AreEqual(4, top.Edges().Count());
            Assert.AreEqual(4, left.Edges().Count());
            Assert.AreEqual(4, right.Edges().Count());
            Assert.AreEqual(4, front.Edges().Count());
            Assert.AreEqual(4, back.Edges().Count());

            Assert.AreEqual(0, mesh.BoundaryFace.Edges().Count());

            Assert.AreEqual(6, mesh.Faces.Count());
            Assert.AreEqual(8, mesh.Vertices.Count());
            Assert.AreEqual(24, mesh.HalfEdges.Count());

            foreach (var vtx in mesh.Vertices)
            {
                Assert.AreEqual(3, vtx.AdjacentEdges().Count());
                Assert.AreEqual(3, vtx.AdjacentVertices().Count());
            }
        }

        [TestMethod]
        public void TestInvalidSquareRayed1()
        {
            var mesh = new RayedMesh<double>(2);

            // ReSharper disable InconsistentNaming
            var ptLL = mesh.AddVertex(0, 0);
            var ptLR = mesh.AddVertex(1, 0);
            var ptUL = mesh.AddVertex(0, 1);
            var ptUR = mesh.AddVertex(1, 1);
            // ReSharper restore InconsistentNaming
            mesh.AddFace(ptLL, ptLR, ptUR, ptUL);

            var failed = false;
            try
            {
                // No valid boundary
                mesh.FinalizeMesh();
            }
            catch (Exception)
            {
                failed = true;
            }
            Assert.IsTrue(failed);
        }

        [TestMethod]
        public void TestInvalidSquareRayed2()
        {
            var mesh = new RayedMesh<double>(2);
            var ptLL = mesh.AddVertex(0, 0);
            var ptLR = mesh.AddVertex(1, 0);
            var ptUL = mesh.AddVertex(0, 1);
            var ptUR = mesh.AddVertex(1, 1);
            mesh.AddFace(ptLL, ptLR, ptUR, ptUL);

            // Adding one ray is invalid
            var ptllRayed = mesh.AddRayedVertex(-1, -1);

            var failed = false;
            try
            {
                // Only one ray in a face is invalid
                mesh.AddFace(ptLL, ptLR, ptllRayed);
            }
            catch (Exception)
            {
                failed = true;
            }
            Assert.IsTrue(failed);
        }

        [TestMethod]
        public void TestInvalidSquareRayed3()
        {
            var mesh = new RayedMesh<double>(2);
            var ptLL = mesh.AddVertex(0, 0);
            var ptLR = mesh.AddVertex(1, 0);
            var ptUL = mesh.AddVertex(0, 1);
            var ptUR = mesh.AddVertex(1, 1);
            mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
            // Adding one ray is invalid
            var ptllRayed = mesh.AddRayedVertex(-1, -1);
            var ptlrRayed = mesh.AddRayedVertex(1, -1);
            mesh.AddFace(ptLR, ptLL, ptllRayed, ptlrRayed);

            var failed = false;
            try
            {
                // One face with two rays doesn't complete the boundary
                mesh.FinalizeMesh();
            }
            catch (Exception)
            {
                failed = true;
            }
            Assert.IsTrue(failed);
        }

        [TestMethod]
        public void TestValidSquareRayed()
        {
            var mesh = new RayedMesh<double>(2);
            var ptLL = mesh.AddVertex(0, 0);
            var ptLR = mesh.AddVertex(1, 0);
            var ptUL = mesh.AddVertex(0, 1);
            var ptUR = mesh.AddVertex(1, 1);
            mesh.AddFace(ptLL, ptLR, ptUR, ptUL);
            // Adding one ray is invalid
            var ptllRayed = mesh.AddRayedVertex(-1, -1);
            var ptlrRayed = mesh.AddRayedVertex(1, -1);
            var pturRayed = mesh.AddRayedVertex(1, 1);
            var ptulRayed = mesh.AddRayedVertex(-1, 1);
            mesh.AddFace(ptLR, ptLL, ptllRayed, ptlrRayed);
            mesh.AddFace(ptUR, ptLR, ptlrRayed, pturRayed);
            mesh.AddFace(ptUL, ptUR, pturRayed, ptulRayed);
            mesh.AddFace(ptLL, ptUL, ptulRayed, ptllRayed);

            var failed = false;
            try
            {
                // One face with two rays doesn't complete the boundary
                mesh.FinalizeMesh();
            }
            catch (Exception)
            {
                failed = true;
            }
            Assert.IsFalse(failed);
        }

    }
}
