using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using mexLib.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MexManager.Controls
{
    public class SelectCanvas : Control
    {
        public static readonly StyledProperty<ObservableCollection<MexCharacterSelectIcon>> IconsProperty =
            AvaloniaProperty.Register<SelectCanvas, ObservableCollection<MexCharacterSelectIcon>>(nameof(Icons));

        public static readonly StyledProperty<float> ScaleProperty =
            AvaloniaProperty.Register<SelectCanvas, float>(nameof(Scale));

        public ObservableCollection<MexCharacterSelectIcon> Icons
        {
            get => GetValue(IconsProperty);
            set 
            {
                SetValue(IconsProperty, value);
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

        private MexCharacterSelectIcon? _draggingIcon;
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

            context.FillRectangle(Brushes.Red, new Rect(Bounds.Size));

            if (Icons != null)
            {
                foreach (var icon in Icons)
                {
                    DrawIcon(context, icon);
                }
            }

            var rect = TransformRect(0, 0, 35.05f, 28.8f);
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

        private void DrawIcon(DrawingContext context, MexCharacterSelectIcon icon)
        {
            {
                var width = MexCharacterSelectIcon.BaseWidth * icon.ScaleX;
                var height = MexCharacterSelectIcon.BaseHeight * icon.ScaleY;

                var rect = TransformRect(icon.X, icon.Y, width, height);
                var brush = Brushes.White; // Customize this as needed.
                context.FillRectangle(brush, rect);
            }
            {
                var rect = TransformRect(icon.X + icon.CollisionOffsetX, icon.Y + icon.CollisionOffsetY, icon.CollisionSizeX / 2 * icon.ScaleX, icon.CollisionSizeY / 2 * icon.ScaleY);
                var pen = new Pen(Brushes.Black, 1); // Black color and 2 pixels thick
                context.DrawRectangle(Brushes.Transparent, pen, rect);
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
                    _dragStart = position;
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

                _draggingIcon.X += (float)delta.X / Scale;
                _draggingIcon.Y -= (float)delta.Y / Scale;
                _dragStart = position;

                InvalidateVisual();
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            _draggingIcon = null;
        }
    }
}