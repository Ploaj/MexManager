using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using mexLib;
using mexLib.Types;
using MexManager.Tools;
using PropertyModels.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MexManager.Controls
{
    public class SelectCanvasDisplayProperties : ReactiveObject
    {
        private float _zoom = 8;
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
        public float XOffset
        {
            get => _offsetX;
            set
            {
                this.RaiseAndSetIfChanged(ref _offsetX, value);
            }
        }

        private float _offsetY = 0;
        public float YOffset
        {
            get => _offsetY;
            set
            {
                this.RaiseAndSetIfChanged(ref _offsetY, value);
            }
        }

        private bool _showCollision = true;
        public bool ShowCollision
        {
            get => _showCollision;
            set => this.RaiseAndSetIfChanged(ref _showCollision, value);
        }
    }

    public class SelectCanvas : ItemsControl
    {
        public static readonly StyledProperty<IImage> TemplateImageProperty =
            AvaloniaProperty.Register<SelectCanvas, IImage>(nameof(TemplateImage));

        public static readonly StyledProperty<object?> SelectedIconProperty =
            AvaloniaProperty.Register<SelectCanvas, object?>(nameof(SelectedIcon));

        public static readonly StyledProperty<bool> SwapModeProperty =
            AvaloniaProperty.Register<SelectCanvas, bool>(nameof(SwapMode));

        private readonly static float HandWidth = 7.2f;

        private readonly static float HandHeight = 9.6f;

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
            public MexCharacterSelectIcon Icon;

            public float X;
            public float Y;
            public float Z;

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

        private readonly Stack<ObservableCollection<IconState>> _undoStack = new ();
        private readonly Stack<ObservableCollection<IconState>> _redoStack = new ();


        private Point _cursorPosition = new Point();

        public SelectCanvas()
        {
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
                var current = e.GetPosition(this);
                var delta = current - _cursorPosition;
                _cursorPosition = current;

                if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
                {
                    Properties.XOffset += (float)delta.X;
                    Properties.YOffset += (float)delta.Y;
                    InvalidateVisual();
                }
            };
        }

        public List<MexIconBase> Icons
        {
            get => Items.OfType<MexIconBase>().ToList();
        }

        private void PushState(Stack<ObservableCollection<MexIconBase>> stack)
        {
            var iconsCopy = new ObservableCollection<MexIconBase>(Icons);
            stack.Push(iconsCopy);
        }

        public void Undo()
        {
            //if (_undoStack.Count > 0)
            //{
            //    PushState(_redoStack); // Save current state to redo stack
            //    Icons = _undoStack.Pop(); // Restore state from undo stack
            //}
        }

        public void Redo()
        {
            //if (_redoStack.Count > 0)
            //{
            //    PushState(_undoStack); // Save current state to undo stack
            //    Icons = _redoStack.Pop(); // Restore state from redo stack
            //}
        }

        public void BeginChange()
        {
            //Debug.WriteLine("Begin");
            //PushState(_undoStack); // Save current state to undo stack
            //_redoStack.Clear(); // Clear redo stack
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Create a translation matrix to move everything
            //var translationMatrix = Matrix.CreateTranslation(Properties.XOffset, Properties.YOffset);
            //using var pop = context.PushTransform(translationMatrix);

            // draw background
            var rect = TransformRect(0, 0, 35.05f, 28.8f);
            context.DrawImage(TemplateImage, rect);

            // draw icons
            if (Icons != null)
            {
                foreach (var icon in Icons)
                {
                    DrawIcon(context, icon);
                }
            }

            // draw ghost icon
            if (_draggingIcon != null && SwapMode)
                DrawIconGhost(context, _draggingIcon);

            // highlight selected icon
            if (SelectedIcon != null)
                DrawIconCollision(context, SelectedIcon as MexIconBase, Brushes.Yellow);

            // draw hand
            //DrawCursorHand(context);
        }

        private Rect TransformRect(float x, float y, float w, float h)
        {
            var viewportWidth = Bounds.Width;
            var viewportHeight = Bounds.Height;

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

        private Bitmap? GetIconBitmap(MexIconBase icon)
        {
            if (Global.Workspace != null)
            {
                if (!IconBitmapCache.ContainsKey(icon.ImageKey))
                {
                    var tex = icon.GetIconImage(Global.Workspace);

                    if (tex != null)
                        IconBitmapCache.Add(icon.ImageKey, tex.ToBitmap());
                }

                if (IconBitmapCache.ContainsKey(icon.ImageKey))
                    return IconBitmapCache[icon.ImageKey];
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        private void DrawIconCollision(DrawingContext context, MexIconBase? icon, IBrush color)
        {
            if (icon == null)
                return;

            var off = icon.CollisionOffset;
            var size = icon.CollisionSize;

            var rect = TransformRect(
                icon.X + off.Item1,
                icon.Y + off.Item2,
                size.Item1 * icon.ScaleX,
                size.Item2 * icon.ScaleY);
            var pen = new Pen(color, 2);
            context.DrawRectangle(Brushes.Transparent, pen, rect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="icon"></param>
        private void DrawIcon(DrawingContext context, MexIconBase icon)
        {
            var width = icon.BaseWidth * icon.ScaleX;
            var height = icon.BaseHeight * icon.ScaleY;

            var rect = TransformRect(icon.X, icon.Y, width, height);

            var bmp = GetIconBitmap(icon);
            if (bmp == null)
            {
                context.FillRectangle(Brushes.White, rect);
            }
            else
            {
                context.DrawImage(bmp, rect);
            }

            if (Properties.ShowCollision)
                DrawIconCollision(context, icon, Brushes.White);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="icon"></param>
        private void DrawIconGhost(DrawingContext context, MexIconBase icon)
        {
            var width = icon.BaseWidth * icon.ScaleX;
            var height = icon.BaseHeight * icon.ScaleY;

            var rect = TransformRect((float)_ghostPointX, (float)_ghostPointY, width, height);

            var bmp = GetIconBitmap(icon);
            if (bmp == null)
            {
                var pen = new Pen(Brushes.Yellow, 1);
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
                using var op = context.PushOpacity(0.5);
                context.DrawImage(bmp, rect);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        private void DrawCursorHand(DrawingContext context)
        {
            var width = HandWidth * Properties.Zoom;
            var height = HandHeight * Properties.Zoom;
            var rect = new Rect((float)_cursorPosition.X - width / 2, (float)_cursorPosition.Y - height / 2, width, height);
            context.DrawImage(BitmapManager.CSSHandPoint, rect);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private MexIconBase? GetIconAtPosition(Point position)
        {
            foreach (var icon in Icons)
            {
                var width = icon.BaseWidth * icon.ScaleX;
                var height = icon.BaseHeight * icon.ScaleY;
                var rect = TransformRect(icon.X, icon.Y, width, height);

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

            BeginChange(); // Begin a new change for undo/redo

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var position = e.GetPosition(this);
                var icon = GetIconAtPosition(position);
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
                var position = e.GetPosition(this);
                var delta = position - _dragStart;

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
                var position = e.GetPosition(this);
                var icon = GetIconAtPosition(position);

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