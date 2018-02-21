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
	                    var vtxCur = new Vector(aiVtx.X, aiVtx.Y, aiVtx.Z);
                        var vtx = mesh.AddVertex(vtxCur);
						if (addNormals)
						{
							var v3d = aiMesh.Normals[iVtx];
							vtx.Normal = new Vector(v3d.X, v3d.Y, v3d.Z);
						}
					}
                    foreach (var face in aiMesh.Faces)
	                {
		                mesh.AddFace(face.Indices);
	                }
                    mesh.FinalizeMesh();
	                mesh.Validate();
                }
            }
	        return ret;
        }
        #endregion
    }
}
