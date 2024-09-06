using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using mexLib.Attributes;
using PropertyModels.Extensions;
using System;

namespace MexManager.Factories
{
    public class HexCellFactory : AbstractCellEditFactory
    {
        public override int ImportPriority => base.ImportPriority - 100000;

        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            var prop = context.Property;

            if (prop.GetCustomAttribute<DisplayHexAttribute>() == null)
                return null;

            if (prop.PropertyType == typeof(uint))
            {
                var control = new UIntHexEditor()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                if (prop.GetValue(context.Target) is uint value)
                    control.Value = value;

                control.OnValueChanged += (e) =>
                {
                    prop.SetValue(context.Target, e);
                };

                return control;
            }

            return null; // Return null for other types, letting the default editor handle them.
        }

        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            return false;
        }
    }
    public class UIntHexEditor : UserControl
    {
        private readonly TextBox _textBox;

        // Define an event for value changes
        public event Action<uint>? OnValueChanged;

        public UIntHexEditor()
        {
            _textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            _textBox.TextChanged += OnTextChanged;

            Content = _textBox;
        }

        public uint Value
        {
            get => _value;
            set
            {
                _textBox.Text = $"0x{value:X}";
                _value = value;
                OnValueChanged?.Invoke(value);
            }
        }
        private uint _value;

        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            string? text = _textBox.Text;

            // Remove the '0x' prefix if it exists
            if (!string.IsNullOrEmpty(text) &&
                text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                text = text[2..];
            }

            if (uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out uint result))
            {
                Value = result;
            }
            else
            {
                Value = _value;
            }
        }
    }
}
