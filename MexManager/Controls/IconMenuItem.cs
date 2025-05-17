using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace MexManager.Controls
{
    public class IconMenuItem : MenuItem
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<IconMenuItem, string>(nameof(Text));

        public static readonly StyledProperty<string> IconSourceProperty =
            AvaloniaProperty.Register<IconMenuItem, string>(nameof(IconSource));

        private readonly Image image;

        public string IconSource
        {
            get => GetValue(IconSourceProperty);
            set
            {
                SetValue(IconSourceProperty, value);
                image.Source = new Bitmap(AssetLoader.Open(new Uri(value)));
            }
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                if (string.IsNullOrEmpty(value))
                {
                    textBlock.IsVisible = false;
                }
            }
        }

        private readonly TextBlock textBlock;

        public IconMenuItem()
        {
            StackPanel stackPanel = new()
            {
                Orientation = Orientation.Vertical,
            };

            image = new Image()
            {
                Width = 24,
                Height = 24,
            };

            textBlock = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            textBlock[!TextBlock.TextProperty] = this[!TextProperty];

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            this.Header = stackPanel;
        }
    }
}
