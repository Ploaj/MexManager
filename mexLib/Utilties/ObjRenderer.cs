using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Numerics;

namespace mexLib.Utilties
{
    public class ObjRenderer
    {
        private readonly ObjFile _objFile;
        private readonly int _imageWidth;
        private readonly int _imageHeight;

        public ObjRenderer(ObjFile objFile, int imageWidth, int imageHeight)
        {
            _objFile = objFile;
            _imageWidth = imageWidth;
            _imageHeight = imageHeight;
        }

        // Method to render the OBJ data onto the image
        public static Image<Rgba32> Render(ObjFile _objFile, int _imageWidth, int _imageHeight, Color color)
        {
            // Create an empty image
            var image = new Image<Rgba32>(_imageWidth, _imageHeight , Color.Transparent);

            // Project 3D vertices to 2D
            List<Vector2> projectedVertices = _objFile.Vertices.Select(e => new Vector2(e.X, e.Y)).ToList();

            // Compute the bounding box of the projected vertices
            (Vector2 min, Vector2 max) boundingBox = GetBoundingBox(projectedVertices);

            // Scale and translate vertices to fit within the image, centered
            List<Vector2> transformedVertices = TransformVertices(_imageWidth, _imageHeight, projectedVertices, boundingBox);

            var options =
                new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions()
                    {
                        BlendPercentage = 1,                   // Apply full blending
                        Antialias = false,                      // Smooth edges
                        AntialiasSubpixelDepth = 1,
                        ColorBlendingMode = PixelColorBlendingMode.Normal,  // Correct blending for premultiplied alpha
                        AlphaCompositionMode = PixelAlphaCompositionMode.DestOver // Classic alpha blending
                    },
                };

            // Draw the faces
            foreach (var face in _objFile.Faces)
            {
                // Get the vertices for this face
                var polygonVertices = face.Vertices
                                          .Select(v => transformedVertices[v.VertexIndex])
                                          .Select(v => new PointF(v.X, _imageHeight - v.Y))
                                          .ToArray();

                // Fill the polygon 
                image.Mutate(ctx => ctx.FillPolygon(options, color, polygonVertices));
            }

            return image;
        }

        // Get the bounding box of the projected vertices
        private static (Vector2 min, Vector2 max) GetBoundingBox(List<Vector2> vertices)
        {
            float minX = vertices.Min(v => v.X);
            float minY = vertices.Min(v => v.Y);
            float maxX = vertices.Max(v => v.X);
            float maxY = vertices.Max(v => v.Y);
            return (new Vector2(minX, minY), new Vector2(maxX, maxY));
        }

        // Transform vertices to fit within the image and center them
        private static List<Vector2> TransformVertices(int _imageWidth, int _imageHeight, List<Vector2> vertices, (Vector2 min, Vector2 max) boundingBox)
        {
            Vector2 size = boundingBox.max - boundingBox.min;
            float scale = Math.Min(_imageWidth / size.X, _imageHeight / size.Y);

            Vector2 center = (boundingBox.min + boundingBox.max) / 2;
            Vector2 imageCenter = new Vector2(_imageWidth / 2, _imageHeight / 2);

            return vertices.Select(v =>
            {
                Vector2 scaled = (v - center) * scale;
                return scaled + imageCenter;
            }).ToList();
        }
    }
}
