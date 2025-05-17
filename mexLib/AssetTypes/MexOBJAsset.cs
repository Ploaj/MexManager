using HSDRaw.Common;
using HSDRaw.GX;
using mexLib.Utilties;
using System.IO.Compression;

namespace mexLib.AssetTypes
{
    public class MexOBJAsset
    {
        public string? AssetFileName { get; set; }

        public string AssetPath { get; internal set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public MexOBJAsset()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        private static string GetRelativePath(string basePath, string subPath)
        {
            Uri baseUri = new(basePath);
            Uri subUri = new(subPath);
            Uri relativeUri = baseUri.MakeRelativeUri(subUri);

            // Get the relative path string
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));

            // Remove the file extension, if present
            string relativePathWithoutExtension = Path.Combine(Path.GetDirectoryName(relativePath) ?? string.Empty,
                                                               Path.GetFileNameWithoutExtension(relativePath));

            return relativePathWithoutExtension;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        private string GetUniqueAssetPath(MexWorkspace workspace)
        {
            string assetPath = workspace.GetAssetPath("");
            string sourcePath = workspace.FileManager.GetUniqueFilePath(workspace.GetAssetPath(AssetPath) + ".obj");
            return GetRelativePath(assetPath, sourcePath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public string GetFullPath(MexWorkspace workspace)
        {
            AssetFileName ??= GetUniqueAssetPath(workspace);
            return workspace.GetAssetPath(AssetFileName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="image"></param>
        public void SetFromData(MexWorkspace workspace, byte[] data)
        {
            string path = GetFullPath(workspace);

            // set data
            workspace.FileManager.Set(path + ".obj", data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="obj"></param>
        public void SetFromObjFile(MexWorkspace workspace, ObjFile obj)
        {
            string path = GetFullPath(workspace);

            // set data
            using MemoryStream stream = new();
            obj.Write(stream);
            workspace.FileManager.Set(path + ".obj", stream.ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dobj"></param>
        public void SetFromDObj(MexWorkspace workspace, HSD_DOBJ dobj)
        {
            string path = GetFullPath(workspace);

            // convert dobj data to position only obj file
            ObjFile obj = new();
            GX_DisplayList dl = dobj.Pobj.ToDisplayList();

            int offset = 0;
            foreach (GX_PrimitiveGroup? prim in dl.Primitives)
            {
                List<GX_Vertex> verts = dl.Vertices.GetRange(offset, prim.Count);
                offset += prim.Count;

                switch (prim.PrimitiveType)
                {
                    case GXPrimitiveType.Quads:
                        verts = TriangleTools.QuadToList(verts);
                        break;
                    case GXPrimitiveType.TriangleStrip:
                        verts = TriangleTools.StripToList(verts);
                        break;
                    case GXPrimitiveType.Triangles:
                        break;
                    default:
                        throw new NotSupportedException(prim.PrimitiveType.ToString() + " not supported");
                }

                for (int i = 0; i < verts.Count; i += 3)
                {
                    // add index
                    obj.Faces.Add(new ObjFile.Face()
                    {
                        Vertices =
                    {
                        new ()
                        {
                            VertexIndex = obj.Vertices.Count,
                        },
                        new ()
                        {
                            VertexIndex = obj.Vertices.Count + 1,
                        },
                        new ()
                        {
                            VertexIndex = obj.Vertices.Count + 2,
                        },
                    }
                    });

                    // add position
                    obj.Vertices.Add(new ObjFile.Vector3(verts[i].POS.X, verts[i].POS.Y, verts[i].POS.Z));
                    obj.Vertices.Add(new ObjFile.Vector3(verts[i + 1].POS.X, verts[i + 1].POS.Y, verts[i + 1].POS.Z));
                    obj.Vertices.Add(new ObjFile.Vector3(verts[i + 2].POS.X, verts[i + 2].POS.Y, verts[i + 2].POS.Z));
                }
            }
            using MemoryStream stream = new();
            obj.Write(stream);
            workspace.FileManager.Set(path + ".obj", stream.ToArray());

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public ObjFile? GetOBJFile(MexWorkspace workspace)
        {
            if (AssetFileName == null)
                return null;

            string path = GetFullPath(workspace);

            using Stream? stream = workspace.FileManager.GetStream(path + ".obj");
            if (stream == null)
                return null;

            ObjFile obj = new();
            obj.Load(stream);
            return obj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        public void Delete(MexWorkspace workspace)
        {
            if (AssetFileName == null)
                return;

            string path = GetFullPath(workspace);

            workspace.FileManager.Remove(path + ".obj");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="zip"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool SetFromPackage(MexWorkspace workspace, ZipArchive zip, string filename)
        {
            ZipArchiveEntry? entry = zip.GetEntry(filename);

            if (entry == null)
                return false;

            AssetFileName = null;
            SetFromData(workspace, entry.Extract());
            return true;
        }
    }
}
