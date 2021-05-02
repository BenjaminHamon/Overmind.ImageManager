using System.Windows;
using System.Windows.Media;

namespace Overmind.WpfExtensions
{
	public static class VisualTreeExtensions
	{
		public static TVisual GetDescendant<TVisual>(Visual element)
			where TVisual : Visual
		{
			if (element is FrameworkElement)
			{
				((FrameworkElement)element).ApplyTemplate();
			}

			for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(element); childIndex += 1)
			{
				Visual child = VisualTreeHelper.GetChild(element, childIndex) as Visual;

				if (child != null)
				{
					if (child is TVisual)
						return (TVisual)child;

					TVisual descendant = GetDescendant<TVisual>(child);
					if (descendant != null)
						return descendant;
				}
			}

			return null;
		}

		public static TVisual GetAncestor<TVisual>(Visual element)
			where TVisual : Visual
		{
			Visual parent = VisualTreeHelper.GetParent(element) as Visual;

			if (parent == null)
				return null;
			if (parent is TVisual)
				return (TVisual)parent;

			return GetAncestor<TVisual>(parent);
		}
	}
}
