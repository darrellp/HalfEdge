using System;
using System.Collections.Generic;
using Assimp;

namespace MeshNav
{
	public static class Import
    {
        #region Importing

        public static List<Mesh<T>> ImportMesh<T>(HalfEdgeFactory<T> factory, string filename) where T : struct, IEquatable<T>, IFormattable
		{
			var ret = new List<Mesh<T>>();
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
                        var vtx = mesh.AddVertex(mesh.HalfEdgeFactory.FromVector3D(aiVtx));
						if (addNormals)
						{
							vtx.Normal = mesh.HalfEdgeFactory.FromVector3D(aiMesh.Normals[iVtx]);
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
