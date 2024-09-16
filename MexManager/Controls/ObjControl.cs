using Avalonia.Media;
using mexLib.AssetTypes;
using Avalonia.Controls;
using Avalonia;
using System;
using MexManager.Tools;

namespace MexManager.Controls
{
    public class ObjControl : Control
    {
        private ObjRasterizer Raster;

        private Size _controlSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        public ObjControl(MexOBJAsset asset)
        {
            Raster = new ObjRasterizer(asset);
            RefreshRender();
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshRender()
        {
            if (Global.Workspace == null)
                return;

            Raster.RefreshRender();

            InvalidateVisual();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dc"></param>
        public override void Render(DrawingContext dc)
        {
            base.Render(dc);
            Raster.RenderEmblem(dc, Bounds.Width, Math.Min(Bounds.Height, Height) - 60, 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Refresh render when the size changes
            if (e.Property == WidthProperty || e.Property == HeightProperty)
            {
                _controlSize = new Size(Width, Height);
                RefreshRender();
                InvalidateVisual(); // Request a redraw
            }
        }
    }
}
