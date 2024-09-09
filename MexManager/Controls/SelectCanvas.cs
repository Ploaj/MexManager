using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using mexLib;
using mexLib.Types;
using MexManager.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MexManager.Controls
{
    public class SelectCanvas : Control
    {
        public static readonly StyledProperty<ObservableCollection<MexCharacterSelectIcon>> IconsProperty =
            AvaloniaProperty.Register<SelectCanvas, ObservableCollection<MexCharacterSelectIcon>>(nameof(Icons));

        public static readonly StyledProperty<MexCharacterSelectIcon> SelectedIconProperty =
            AvaloniaProperty.Register<SelectCanvas, MexCharacterSelectIcon>(nameof(Icons));

        public static readonly StyledProperty<float> ScaleProperty =
            AvaloniaProperty.Register<SelectCanvas, float>(nameof(Scale));

        public static readonly StyledProperty<bool> SwapModeProperty =
            AvaloniaProperty.Register<SelectCanvas, bool>(nameof(SwapMode));

        public ObservableCollection<MexCharacterSelectIcon> Icons
        {
            get => GetValue(IconsProperty);
            set 
            {
                SetValue(IconsProperty, value);
                InvalidateVisual();
            }
        }

        public MexCharacterSelectIcon SelectedIcon
        {
            get => GetValue(SelectedIconProperty);
            set
            {
                SetValue(SelectedIconProperty, value);
                InvalidateVisual();
            }
        }

        public float Scale
        {
            get => GetValue(ScaleProperty);
            set
            {
                SetValue(ScaleProperty, value);
                InvalidateVisual();
            }
        }

        public bool SwapMode
        {
            get => GetValue(SwapModeProperty);
            set
            {
                SetValue(SwapModeProperty, value);
            }
        }

        public delegate void SwapDelegate();
        public SwapDelegate OnSwap;

        private MexCharacterSelectIcon? _draggingIcon;
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

        private Stack<ObservableCollection<IconState>> _undoStack = new Stack<ObservableCollection<IconState>>();
        private Stack<ObservableCollection<IconState>> _redoStack = new Stack<ObservableCollection<IconState>>();

        public SelectCanvas()
        {

        }

        private void PushState(Stack<ObservableCollection<MexCharacterSelectIcon>> stack)
        {
            var iconsCopy = new ObservableCollection<MexCharacterSelectIcon>(Icons);
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

            var rect = TransformRect(0, 0, 35.05f, 28.8f);

            context.DrawImage(BitmapManager.CSSTemplate, rect);

            if (Icons != null)
            {
                foreach (var icon in Icons)
                {
                    DrawIcon(context, icon);
                }
            }

            if (_draggingIcon != null && SwapMode)
            {
                DrawIconGhost(context, _draggingIcon);
            }

            var brush = Brushes.Transparent; // Customize this as needed.
            var pen = new Pen(Brushes.Black, 2); // Black color and 2 pixels thick
            context.DrawRectangle(brush, pen, rect);
        }

        private Rect TransformRect(float x, float y, float w, float h)
        {
            var viewportWidth = Width;
            var viewportHeight = Height;

            x *= Scale;
            y *= Scale;
            w *= Scale;
            h *= Scale;

            return new Rect(
                viewportWidth / 2 + x - w, 
                viewportHeight / 2 - y - h, 
                w * 2, 
                h * 2);
        }

        private Dictionary<MexFighter, Bitmap> fighterToIcon = new Dictionary<MexFighter, Bitmap>();

        private Bitmap? GetIconBitmap(MexCharacterSelectIcon icon)
        {
            if (Global.Workspace != null)
            {
                var internalId = MexFighterIDConverter.ToInternalID(icon.Fighter, Global.Workspace.Project.Fighters.Count);
                var fighter = Global.Workspace.Project.Fighters[internalId];

                if (!fighterToIcon.ContainsKey(fighter))
                {
                    var tex = fighter.Assets.CSSIconAsset.GetTexFile(Global.Workspace);

                    if (tex != null)
                    {
                        var img = tex.ToBitmap();
                        fighterToIcon.Add(fighter, img);
                    }
                }

                if (fighterToIcon.ContainsKey(fighter))
                    return fighterToIcon[fighter];
            }

            return null;
        }

        private void DrawIcon(DrawingContext context, MexCharacterSelectIcon icon)
        {
            {
                var width = MexCharacterSelectIcon.BaseWidth * icon.ScaleX;
                var height = MexCharacterSelectIcon.BaseHeight * icon.ScaleY;

                var rect = TransformRect(icon.X, icon.Y, width, height);

                var bmp = GetIconBitmap(icon);
                if (bmp == null)
                {
                    var brush = Brushes.White; // Customize this as needed.
                    context.FillRectangle(brush, rect);
                }
                else
                {
                    context.DrawImage(bmp, rect);
                }
            }
            {
                var rect = TransformRect(icon.X + icon.CollisionOffsetX, icon.Y + icon.CollisionOffsetY, icon.CollisionSizeX / 2 * icon.ScaleX, icon.CollisionSizeY / 2 * icon.ScaleY);
                var pen = new Pen(SelectedIcon == icon ? Brushes.Yellow : Brushes.Black, 1); // Black color and 2 pixels thick
                context.DrawRectangle(Brushes.Transparent, pen, rect);
            }
        }
        private void DrawIconGhost(DrawingContext context, MexCharacterSelectIcon icon)
        {
            var width = MexCharacterSelectIcon.BaseWidth * icon.ScaleX;
            var height = MexCharacterSelectIcon.BaseHeight * icon.ScaleY;

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
                context.PushOpacity(0.5);
                context.DrawImage(bmp, rect);
                context.PushOpacity(1.0);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            BeginChange(); // Begin a new change for undo/redo

            var position = e.GetPosition(this);
            foreach (var icon in Icons)
            {
                var width = MexCharacterSelectIcon.BaseWidth * icon.ScaleX;
                var height = MexCharacterSelectIcon.BaseHeight * icon.ScaleY;
                var rect = TransformRect(icon.X, icon.Y, width, height);

                if (rect.Contains(position))
                {
                    _draggingIcon = icon;
                    SelectedIcon = icon;
                    _dragStart = position;
                    _ghostPointX = icon.X;
                    _ghostPointY = icon.Y;
                    e.Handled = true;
                    break;
                }
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (_draggingIcon != null)
            {
                var position = e.GetPosition(this);
                var delta = position - _dragStart;

                if (SwapMode)
                {
                    _ghostPointX += (float)delta.X / Scale;
                    _ghostPointY -= (float)delta.Y / Scale;

                    int swap_index = -1;
                    foreach (var i in Icons)
                    {
                        if (i == _draggingIcon)
                            continue;

                        if ((_ghostPointX - i.X) * (_ghostPointX - i.X) + (_ghostPointY - i.Y) * (_ghostPointY - i.Y) < (10 * 10 / Scale))
                        {
                            swap_index = Icons.IndexOf(i);
                            break;
                        }
                    }
                    if (swap_index != -1)
                    {
                        int myIndex = Icons.IndexOf(_draggingIcon);
                        (Icons[myIndex], Icons[swap_index]) = (Icons[swap_index], Icons[myIndex]);
                        OnSwap?.Invoke();
                    }
                }
                else
                {
                    _draggingIcon.X += (float)delta.X / Scale;
                    _draggingIcon.Y -= (float)delta.Y / Scale;
                }
                _dragStart = position;

                InvalidateVisual();
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            _draggingIcon = null;
            InvalidateVisual();
        }
    }
}