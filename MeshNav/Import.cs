using System;
using System.Collections.Generic;
using System.IO;
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
                    foreach (var vtx in aiMesh.Vertices)
                    {
                        mesh.AddVertex(mesh.HalfEdgeFactory.FromVector3D(vtx));
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
