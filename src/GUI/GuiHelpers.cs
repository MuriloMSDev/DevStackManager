using System.Windows;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// General utility and helper methods for GUI operations.
    /// Provides methods for navigating the WPF visual tree and finding controls.
    /// </summary>
    public static class GuiHelpers
    {
        /// <summary>
        /// Finds a child control by name in the WPF visual tree.
        /// Performs recursive depth-first search through the visual hierarchy.
        /// </summary>
        /// <typeparam name="T">Type of control to find (must be DependencyObject).</typeparam>
        /// <param name="parent">Parent control to start search from.</param>
        /// <param name="childName">Name of the child control to find.</param>
        /// <returns>Found control of type T, or null if not found.</returns>
        public static T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T? foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T childType && (child as FrameworkElement)?.Name == childName)
                {
                    foundChild = childType;
                    break;
                }
                else
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
            }
            
            return foundChild;
        }

        /// <summary>
        /// Finds a visual child control in the template hierarchy.
        /// Searches for first occurrence of specified type in visual tree.
        /// </summary>
        /// <typeparam name="T">Type of control to find (must be DependencyObject).</typeparam>
        /// <param name="parent">Parent control to start search from.</param>
        /// <returns>Found control of type T, or null if not found.</returns>
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                
                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }
    }
}
