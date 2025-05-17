using Avalonia.Controls;

namespace MexManager.Extensions
{
    public static class ListBoxExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="selected_index"></param>
        public static void RefreshList(this ListBox list, int selected_index = -1)
        {
            // refresh items so that indices update
            System.Collections.IEnumerable? items = list.ItemsSource;
            list.ItemsSource = null;
            list.ItemsSource = items;

            list.SelectedIndex = selected_index;
        }
    }
}
