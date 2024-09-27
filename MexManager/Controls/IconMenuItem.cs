using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Media.Imaging;

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
            var stackPanel = new StackPanel { 
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
