using Avalonia;
using Avalonia.Media;

namespace MexManager.Extensions
{
    internal static class ThemeExtensions
    {
        public static IBrush SystemAccentColor { get => GetThemeBrush("SystemAccentColor"); }

        public static IBrush GetThemeBrush(string resourceKey)
        {
            if (Application.Current != null &&
                Application.Current.Styles.TryGetResource(resourceKey, Avalonia.Styling.ThemeVariant.Dark, out object? resource) &&
                resource is IBrush brush)
            {
                return brush;
            }

            // Return a default brush if not found, or handle as needed
            return Brushes.Transparent;
        }
    }
}
