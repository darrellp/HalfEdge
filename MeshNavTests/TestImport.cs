using MeshNav;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MeshNavTests
{
    [TestClass]
    public class TestImport
    {
        [TestMethod]
        public void TestImportObjFile()
        {
	        var factory = new HalfEdgeFactory<double>(3);
	        var meshes = Import.ImportMesh(factory, "teapot.obj");
			Assert.AreEqual(meshes.Count, 1);
        }
    }
}
