using System.Collections.Generic;
using Assimp;

namespace MeshNav
{
	public static class Import
    {
        #region Importing

        public static List<Mesh> ImportMesh(Factory factory, string filename)
		{
			var ret = new List<Mesh>();
            using (var importer = new AssimpContext())
            {
                var model = importer.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);

                foreach (var aiMesh in model.Meshes)
                {
                    if (!aiMesh.HasVertices)
                    {
                        continue;
                    }
	                var mesh = factory.CreateMesh();
	                ret.Add(mesh);
	                var addNormals = mesh.NormalsTrait && aiMesh.HasNormals;

					for (var iVtx = 0; iVtx < aiMesh.Vertices.Count; iVtx++)
	                {
		                var aiVtx = aiMesh.Vertices[iVtx];
	                    var vtxCur = factory.FromVector3D(aiVtx);
                        var vtx = mesh.AddVertex(vtxCur);
						if (addNormals)
						{
							vtx.Normal = factory.FromVector3D(aiMesh.Normals[iVtx]);
						}
					}
                    foreach (var face in aiMesh.Faces)
	                {
		                mesh.AddFace(face.Indices);
	                }
	                mesh.Validate();
                }
            }
	        return ret;
        }
        #endregion
    }
}
