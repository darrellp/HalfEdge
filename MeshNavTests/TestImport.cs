using MeshNav;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Templates;

namespace MeshNavTests
{
    [TestClass]
    public class TestImport
    {
        [TestMethod]
        public void TestImportObjFile()
        {
	        var factory = new BndFactory(3);
	        var meshes = Import.ImportMesh(factory, "teapot.obj");
			Assert.AreEqual(meshes.Count, 1);
        }
    }
}
