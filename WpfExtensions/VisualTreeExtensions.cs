using System.Windows;
using System.Windows.Media;

namespace Overmind.WpfExtensions
{
	public static class VisualTreeExtensions
	{
		public static TVisual GetDescendant<TVisual>(Visual element)
			where TVisual : Visual
		{
			if (element == null)
				return null;

			if (element is TVisual)
				return (TVisual)element;
			
			if (element is FrameworkElement)
				((FrameworkElement)element).ApplyTemplate();

			for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(element); childIndex += 1)
			{
				Visual child = VisualTreeHelper.GetChild(element, childIndex) as Visual;
				TVisual descendant = GetDescendant<TVisual>(child);
				if (descendant != null)
					return descendant;
			}

			return null;
		}
	}
}
