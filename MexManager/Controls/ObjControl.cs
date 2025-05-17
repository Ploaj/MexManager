using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using mexLib.AssetTypes;
using MexManager.Tools;
using System;

namespace MexManager.Controls
{
    public class ObjControl : Control
    {
        private readonly ObjRasterizer Raster;

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
            Raster.RenderEmblem(dc, Bounds.Width, Math.Min(Bounds.Height, Height), 1);
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
                RefreshRender();
                InvalidateVisual(); // Request a redraw
            }
        }
    }
}
