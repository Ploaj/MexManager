using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using mexLib.Types;
using PropertyModels.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MexManager.Controls
{
    public class SelectCanvasDisplayProperties : ReactiveObject
    {
        private float _zoom = 8;
        [Category("Preview Options")]
        public float Zoom
        {
            get => _zoom;
            set
            {
                if (value < 4)
                    value = 4;
                if (value > 20)
                    value = 20;
                this.RaiseAndSetIfChanged(ref _zoom, value);
            }
        }

        private float _offsetX = 0;
        [Category("Preview Options")]
        [DisplayName("X Offset")]
        public float XOffset
        {
            get => _offsetX;
            set
            {
                this.RaiseAndSetIfChanged(ref _offsetX, value);
            }
        }

        private float _offsetY = 0;
        [Category("Preview Options")]
        [DisplayName("Y Offset")]
        public float YOffset
        {
            get => _offsetY;
            set
            {
                this.RaiseAndSetIfChanged(ref _offsetY, value);
            }
        }

        private bool _showCollision = true;
        [Category("Preview Options")]
        [DisplayName("Show Icon Collisions")]
        public bool ShowCollision
        {
            get => _showCollision;
            set => this.RaiseAndSetIfChanged(ref _showCollision, value);
        }

        private bool _showOverscanArea = false;
        [Category("Preview Options")]
        [DisplayName("Show Overscan Area")]
        public bool ShowOverscanArea
        {
            get => _showOverscanArea;
            set => this.RaiseAndSetIfChanged(ref _showOverscanArea, value);
        }

        [Category("Preview Options")]
        [DisplayName("10% Overscan")]
        public Color OverscanColor10 { get; set; } = Color.FromArgb(180, 0, 255, 0);

        [Category("Preview Options")]
        [DisplayName("5% Overscan")]
        public Color OverscanColor05 { get; set; } = Color.FromArgb(180, 255, 255, 0);

        [Category("Preview Options")]
        [DisplayName("No Overscan/Emulator")]
        public Color OverscanColorNone { get; set; } = Color.FromArgb(180, 255, 0, 0);
    }

    public class SelectCanvas : ItemsControl
    {
        public static readonly StyledProperty<IImage> TemplateImageProperty =
            AvaloniaProperty.Register<SelectCanvas, IImage>(nameof(TemplateImage));

        public static readonly StyledProperty<double> TemplateImageWidthProperty =
            AvaloniaProperty.Register<SelectCanvas, double>(nameof(TemplateImageWidth));

        public static readonly StyledProperty<double> TemplateImageHeightProperty =
            AvaloniaProperty.Register<SelectCanvas, double>(nameof(TemplateImageHeight));

        public static readonly StyledProperty<object?> SelectedIconProperty =
            AvaloniaProperty.Register<SelectCanvas, object?>(nameof(SelectedIcon));

        public static readonly StyledProperty<bool> SwapModeProperty =
            AvaloniaProperty.Register<SelectCanvas, bool>(nameof(SwapMode));

        //private readonly static float HandWidth = 7.2f;

        //private readonly static float HandHeight = 9.6f;

        public double TemplateImageWidth
        {
            get => GetValue(TemplateImageWidthProperty);
            set
            {
                SetValue(TemplateImageWidthProperty, value);
                InvalidateVisual();
            }
        }

        public double TemplateImageHeight
        {
            get => GetValue(TemplateImageHeightProperty);
            set
            {
                SetValue(TemplateImageHeightProperty, value);
                InvalidateVisual();
            }
        }

        public IImage TemplateImage
        {
            get => GetValue(TemplateImageProperty);
            set
            {
                SetValue(TemplateImageProperty, value);
                InvalidateVisual();
            }
        }

        public object? SelectedIcon
        {
            get => GetValue(SelectedIconProperty);
            set
            {
                SetValue(SelectedIconProperty, value);
                InvalidateVisual();
            }
        }

        public SelectCanvasDisplayProperties Properties { get; internal set; } = new SelectCanvasDisplayProperties();

        public bool SwapMode
        {
            get => GetValue(SwapModeProperty);
            set
            {
                SetValue(SwapModeProperty, value);
            }
        }

        public delegate void SwapDelegate(int index1, int index2);
        public SwapDelegate? OnSwap;

        private MexIconBase? _draggingIcon;
        private double _ghostPointX;
        private double _ghostPointY;
        private Point _dragStart;

        private class IconState
        {
            public MexCharacterSelectIcon? Icon;

            public float X;
            public float Y;
            public float Z;

            public IconState()
            {
                Icon = null;
            }

            public IconState(MexCharacterSelectIcon icon)
            {
                Icon = icon;
                X = icon.X;
                Y = icon.Y;
                Z = icon.Z;
            }

            public void Apply(MexCharacterSelectIcon icon)
            {
                icon.X = X;
                icon.Y = Y;
                icon.Z = Z;
            }
        }

        private Point _cursorPosition = new();

        public SelectCanvas()
        {
            TemplateImageWidth = 35.05;
            TemplateImageHeight = 28.8f;

            Properties.PropertyChanged += (s, e) =>
            {
                InvalidateVisual();
            };

            PointerWheelChanged += (sender, e) =>
            {
                Properties.Zoom += (float)e.Delta.Y;
                InvalidateVisual();
            };

            PointerPressed += (sender, e) =>
            {
                _cursorPosition = e.GetPosition(this);
                InvalidateVisual();
            };

            PointerMoved += (sender, e) =>
            {
                Point current = e.GetPosition(this);
                Point delta = current - _cursorPosition;
                _cursorPosition = current;

                if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
                {
                    Properties.XOffset += (float)delta.X;
                    Properties.YOffset += (float)delta.Y;
                    ClampOffset();
                    InvalidateVisual();
                }
            };

            LayoutUpdated += (s, e) =>
            {
                if (s is ItemsControl itemsControl)
                {
                    ClampOffset();
                }
            };
        }

        public void ClampOffset()
        {
            double currentWidth = Bounds.Width / 2;
            double currentHeight = Bounds.Height / 2;

            if (Properties.XOffset < -currentWidth)
                Properties.XOffset = -(float)currentWidth;

            if (Properties.XOffset > currentWidth)
                Properties.XOffset = (float)currentWidth;

            if (Properties.YOffset < -currentHeight)
                Properties.YOffset = -(float)currentHeight;

            if (Properties.YOffset > currentHeight)
                Properties.YOffset = (float)currentHeight;

            if (Math.Abs(Properties.XOffset) <= Single.Epsilon)
                Properties.XOffset = 0;

            if (Math.Abs(Properties.YOffset) <= Single.Epsilon)
                Properties.YOffset = 0;
        }

        public List<MexIconBase> Icons
        {
            get => Items.OfType<MexIconBase>().ToList();
        }

        private readonly static Pen CollisionPen = new(Brushes.White, 2);

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Create a translation matrix to move everything
            //var translationMatrix = Matrix.CreateTranslation(Properties.XOffset, Properties.YOffset);
            //using var pop = context.PushTransform(translationMatrix);

            // draw background
            Rect rect = TransformRect(0, 0, TemplateImageWidth, TemplateImageHeight);
            context.DrawImage(TemplateImage, rect);

            // draw icons
            if (Icons != null)
            {
                foreach (MexIconBase icon in Icons)
                {
                    DrawIcon(context, icon);
                }
            }

            // draw ghost icon
            if (_draggingIcon != null && SwapMode)
                DrawIconGhost(context, _draggingIcon);

            // highlight selected icon
            if (SelectedIcon != null)
                DrawIconSelected(context, SelectedIcon as MexIconBase);

            // draw hand
            //DrawCursorHand(context);

            // overscan area
            if (Properties.ShowOverscanArea)
            {
                Pen OverscanPen = new(new SolidColorBrush(Properties.OverscanColorNone), 2);
                Pen OverscanPen05 = new(new SolidColorBrush(Properties.OverscanColor05), 2);
                Pen OverscanPen10 = new(new SolidColorBrush(Properties.OverscanColor10), 2);

                context.DrawRectangle(OverscanPen, rect);

                Rect overscan = TransformRect(0, 0, TemplateImageWidth * 0.95f, TemplateImageHeight * 0.95f);
                context.DrawRectangle(OverscanPen05, overscan);

                overscan = TransformRect(0, 0, TemplateImageWidth * 0.9f, TemplateImageHeight * 0.9f);
                context.DrawRectangle(OverscanPen10, overscan);
            }
        }

        private Rect TransformRect(double x, double y, double w, double h)
        {
            double viewportWidth = Bounds.Width;
            double viewportHeight = Bounds.Height;

            x *= Properties.Zoom;
            y *= Properties.Zoom;
            w *= Properties.Zoom;
            h *= Properties.Zoom;

            x += Properties.XOffset;
            y -= Properties.YOffset;

            return new Rect(
                viewportWidth / 2 + x - w,
                viewportHeight / 2 - y - h,
                w * 2,
                h * 2);
        }

        private readonly Dictionary<int, Bitmap> IconBitmapCache = [];

        public void RefreshImageCache()
        {
            IconBitmapCache.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        private Bitmap? GetIconBitmap(MexIconBase icon)
        {
            if (Global.Workspace != null)
            {
                if (!IconBitmapCache.ContainsKey(icon.ImageKey))
                {
                    mexLib.MexImage? tex = icon.GetIconImage(Global.Workspace);

                    if (tex != null)
                        IconBitmapCache.Add(icon.ImageKey, tex.ToBitmap());
                }

                if (IconBitmapCache.ContainsKey(icon.ImageKey))
                    return IconBitmapCache[icon.ImageKey];
            }

            return null;
        }
        private readonly static ISolidColorBrush SelectedBrush = new ImmutableSolidColorBrush(Color.FromArgb(100, 255, 255, 0));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        private void DrawIconSelected(DrawingContext context, MexIconBase? icon)
        {
            if (icon == null)
                return;

            (float, float) off = icon.CollisionOffset;
            (float, float) size = icon.CollisionSize;

            Rect rect = TransformRect(
                icon.X + off.Item1,
                icon.Y + off.Item2,
                size.Item1 * icon.ScaleX,
                size.Item2 * icon.ScaleY);
            context.FillRectangle(SelectedBrush, rect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        private void DrawIconCollision(DrawingContext context, MexIconBase? icon)
        {
            if (icon == null)
                return;

            (float, float) off = icon.CollisionOffset;
            (float, float) size = icon.CollisionSize;

            Rect rect = TransformRect(
                icon.X + off.Item1,
                icon.Y + off.Item2,
                size.Item1 * icon.ScaleX,
                size.Item2 * icon.ScaleY);
            context.DrawRectangle(CollisionPen, rect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="icon"></param>
        private void DrawIcon(DrawingContext context, MexIconBase icon)
        {
            float width = icon.BaseWidth * icon.ScaleX;
            float height = icon.BaseHeight * icon.ScaleY;

            Rect rect = TransformRect(icon.X, icon.Y, width, height);

            Bitmap? bmp = GetIconBitmap(icon);
            if (bmp == null)
            {
                context.FillRectangle(Brushes.White, rect);
            }
            else
            {
                context.DrawImage(bmp, rect);
            }

            if (Properties.ShowCollision)
                DrawIconCollision(context, icon);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="icon"></param>
        private void DrawIconGhost(DrawingContext context, MexIconBase icon)
        {
            float width = icon.BaseWidth * icon.ScaleX;
            float height = icon.BaseHeight * icon.ScaleY;

            Rect rect = TransformRect((float)_ghostPointX, (float)_ghostPointY, width, height);

            Bitmap? bmp = GetIconBitmap(icon);
            if (bmp == null)
            {
                Pen pen = new(Brushes.Yellow, 1);
                context.DrawRectangle(Brushes.Transparent, pen, rect);
            }
            else
            {
                //var brush = new ImageBrush
                //{
                //    Source = bmp,
                //    Opacity = 0.5 // Set the desired opacity here
                //};

                // Draw the image with transparency
                using DrawingContext.PushedState op = context.PushOpacity(0.5);
                context.DrawImage(bmp, rect);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        //private void DrawCursorHand(DrawingContext context)
        //{
        //    var width = HandWidth * Properties.Zoom;
        //    var height = HandHeight * Properties.Zoom;
        //    var rect = new Rect((float)_cursorPosition.X - width / 2, (float)_cursorPosition.Y - height / 2, width, height);
        //    context.DrawImage(BitmapManager.CSSHandPoint, rect);
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private MexIconBase? GetIconAtPosition(Point position)
        {
            List<MexIconBase> icons = Icons;
            for (int i = icons.Count - 1; i >= 0; i--)
            {
                MexIconBase icon = icons[i];
                float width = icon.BaseWidth * icon.ScaleX;
                float height = icon.BaseHeight * icon.ScaleY;
                Rect rect = TransformRect(icon.X, icon.Y, width, height);

                if (rect.Contains(position))
                {
                    return icon;
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (Icons == null)
                return;

            //BeginChange(); // Begin a new change for undo/redo

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                Point position = e.GetPosition(this);
                MexIconBase? icon = GetIconAtPosition(position);
                if (icon != null)
                {
                    _draggingIcon = icon;
                    SelectedIcon = icon;
                    _dragStart = position;
                    _ghostPointX = icon.X;
                    _ghostPointY = icon.Y;
                    e.Handled = true;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (_draggingIcon != null)
            {
                Point position = e.GetPosition(this);
                Point delta = position - _dragStart;

                if (SwapMode)
                {
                    _ghostPointX += (float)delta.X / Properties.Zoom;
                    _ghostPointY -= (float)delta.Y / Properties.Zoom;
                }
                else
                {
                    _draggingIcon.X += (float)delta.X / Properties.Zoom;
                    _draggingIcon.Y -= (float)delta.Y / Properties.Zoom;
                }
                _dragStart = position;

                InvalidateVisual();
                e.Handled = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (_draggingIcon != null)
            {
                Point position = e.GetPosition(this);
                MexIconBase? icon = GetIconAtPosition(position);

                if (icon != null)
                {
                    int swap_index = Icons.IndexOf(icon);
                    if (swap_index != -1)
                    {
                        int myIndex = Icons.IndexOf(_draggingIcon);
                        OnSwap?.Invoke(myIndex, swap_index);
                        SelectedIcon = _draggingIcon;
                    }
                }
            }

            _draggingIcon = null;
            InvalidateVisual();
        }
    }
}