using System.Globalization;

namespace mexLib.Utilties
{
    /// <summary>
    /// This is an extremely simplified obj file for storing the emblem vector images
    /// </summary>
    public class ObjFile
    {
        public struct Vector3
        {
            public float X, Y, Z;
            public Vector3(float x, float y, float z)
            {
                X = x; Y = y; Z = z;
            }
        }

        public class Face
        {
            public List<FaceVertex> Vertices { get; set; } = new List<FaceVertex>();

            public override string ToString()
            {
                string result = "";
                foreach (var vertex in Vertices)
                {
                    result += $"{vertex.VertexIndex + 1}// ";///{(vertex.TextureIndex + 1).ToString()}/{(vertex.NormalIndex + 1).ToString()} ";
                }
                return result.TrimEnd();
            }
        }

        public struct FaceVertex
        {
            public int VertexIndex;
            public int TextureIndex;
            public int NormalIndex;

            public FaceVertex(int vertexIndex, int textureIndex, int normalIndex)
            {
                VertexIndex = vertexIndex;
                TextureIndex = textureIndex;
                NormalIndex = normalIndex;
            }
        }
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        public List<Face> Faces { get; set; } = new List<Face>();

        // Load an OBJ file
        public void Load(Stream stream)
        {
            using StreamReader reader = new (stream);
            
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        Vertices.Add(ParseVector3(parts));
                        break;
                    //case "vt":
                    //    TextureCoordinates.Add(ParseVector2(parts));
                    //    break;
                    //case "vn":
                    //    Normals.Add(ParseVector3(parts));
                    //    break;
                    case "f":
                        Faces.Add(ParseFace(parts));
                        break;
                }
            }
        }

        // Save to an OBJ file
        public void Write(Stream stream)
        {
            using StreamWriter writer = new (stream);
            
            foreach (var vertex in Vertices)
            {
                writer.WriteLine($"v {vertex.X.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{vertex.Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{vertex.Z.ToString(CultureInfo.InvariantCulture)}");
            }

            //foreach (var textureCoord in TextureCoordinates)
            //{
            //    writer.WriteLine($"vt {textureCoord.X.ToString(CultureInfo.InvariantCulture)} " +
            //                     $"{textureCoord.Y.ToString(CultureInfo.InvariantCulture)}");
            //}

            //foreach (var normal in Normals)
            //{
            //    writer.WriteLine($"vn {normal.X.ToString(CultureInfo.InvariantCulture)} " +
            //                     $"{normal.Y.ToString(CultureInfo.InvariantCulture)} " +
            //                     $"{normal.Z.ToString(CultureInfo.InvariantCulture)}");
            //}

            foreach (var face in Faces)
            {
                writer.WriteLine($"f {face}");
            }
        }

        private static Vector3 ParseVector3(string[] parts)
        {
            return new Vector3(
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture),
                float.Parse(parts[3], CultureInfo.InvariantCulture)
            );
        }

        private static Face ParseFace(string[] parts)
        {
            var face = new Face();
            for (int i = 1; i < parts.Length; i++)
            {
                var indices = parts[i].Split('/');
                int vertexIndex = int.Parse(indices[0]) - 1;
                //int textureIndex = indices.Length > 1 && indices[1] != "" ? int.Parse(indices[1]) - 1 : -1;
                //int normalIndex = indices.Length > 2 ? int.Parse(indices[2]) - 1 : -1;
                face.Vertices.Add(new FaceVertex(vertexIndex, 0, 0));
            }
            return face;
        }

        public void FlipFaces()
        {
            foreach (var face in Faces)
            {
                face.Vertices.Reverse();
            }
        }
    }
}
