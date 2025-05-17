using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using mexLib.AssetTypes;
using System;
using System.IO;
using System.Linq;

namespace MexManager.Tools
{
    internal class ObjRasterizer
    {
        private readonly MexOBJAsset _asset;

        private StreamGeometry? _render;

        private Size _renderSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj_asset"></param>
        public ObjRasterizer(MexOBJAsset obj_asset)
        {
            _asset = obj_asset;
            RefreshRender();
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshRender()
        {
            if (Global.Workspace == null)
                return;

            _render = new StreamGeometry();

            mexLib.Utilties.ObjFile? obj = _asset.GetOBJFile(Global.Workspace);

            if (obj == null)
                return;

            _renderSize = new Size(
                (obj.Vertices.Max(v => v.X) - obj.Vertices.Min(v => v.X)),
                (obj.Vertices.Max(v => v.Y) - obj.Vertices.Min(v => v.Y)));

            using StreamGeometryContext context = _render.Open();
            foreach (mexLib.Utilties.ObjFile.Face face in obj.Faces)
            {
                mexLib.Utilties.ObjFile.Vector3 v1 = obj.Vertices[face.Vertices[0].VertexIndex];
                mexLib.Utilties.ObjFile.Vector3 v2 = obj.Vertices[face.Vertices[1].VertexIndex];
                mexLib.Utilties.ObjFile.Vector3 v3 = obj.Vertices[face.Vertices[2].VertexIndex];

                Point p1 = new(v1.X, v1.Y);
                Point p2 = new(v2.X, v2.Y);
                Point p3 = new(v3.X, v3.Y);

                context.BeginFigure(p1, true); // Start the figure at the first vertex
                context.LineTo(p2); // Draw line to the second vertex
                context.LineTo(p3); // Draw line to the third vertex
                context.EndFigure(true); // Close the figure to create a closed triangle
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void RenderEmblem(DrawingContext dc, double width, double height, double icon_scale)
        {
            // Draw the triangle
            if (_render != null)
            {
                // Determine scale factors for x and y axes
                double xScale = width / _renderSize.Width;
                double yScale = height / _renderSize.Height;

                double scale = Math.Min(xScale, yScale); // Scale to fit the smallest dimension

                // Determine center of the control
                double centerX = width / 2;
                double centerY = height / 2;

                using DrawingContext.PushedState tra = dc.PushTransform(Matrix.CreateTranslation(centerX, centerY));
                using DrawingContext.PushedState sca2 = dc.PushTransform(Matrix.CreateScale(scale, -scale));
                using DrawingContext.PushedState sca1 = dc.PushTransform(Matrix.CreateScale(icon_scale, icon_scale));

                // Define a brush to fill the triangles
                IBrush brush = Brushes.White;  // You can use a different color or gradient
                                               // Create a StreamGeometry and its context


                dc.DrawGeometry(brush, null, _render);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="filePath"></param>
        public byte[] SaveDrawingToPng(int width, int height)
        {
            // Create a RenderTargetBitmap with the size of the control
            RenderTargetBitmap renderTargetBitmap = new(new PixelSize(width, height), new Vector(96, 96));

            // Render the control into the RenderTargetBitmap
            using DrawingContext context = renderTargetBitmap.CreateDrawingContext();

            // Draw your content here. For demonstration, let's draw the control itself
            RenderEmblem(context, width, height, 0.85);

            // Save the bitmap as PNG
            using MemoryStream stream = new();
            renderTargetBitmap.Save(stream);
            return stream.ToArray();
        }
    }
}
