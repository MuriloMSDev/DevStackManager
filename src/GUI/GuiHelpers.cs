using System.Windows;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Utilitários e helpers gerais para a interface gráfica
    /// </summary>
    public static class GuiHelpers
    {
        /// <summary>
        /// Encontra um controle filho por nome no visual tree
        /// </summary>
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
        /// Encontra um controle visual filho no template
        /// </summary>
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
