namespace RabotoraX.Core.UI;

/// <summary>
/// Represents a layout group that arranges its child elements horizontally, with options for spacing, padding, alignment, and control over child sizes.
/// </summary>
public class UIHorizontalLayoutGroup : UILayoutGroup
{
	protected override void RecalculateHorizontal()
	{
		var children = UILayout.Children.OfType<RUILayout>().ToList();
		if (children.Count == 0) return;

		float innerWidth = UILayout.Rect.Width - PaddingLeft - PaddingRight;
		float[] childWidths = new float[children.Count];
		float totalPreferredWidth = 0;

		for (int i = 0; i < children.Count; i++)
		{
			GetChildPreferredSize(children[i], out float prefW, out _);
			float width = ControlChildWidth ? prefW : children[i].Size.X;
			childWidths[i] = width;
			totalPreferredWidth += width;
		}

		float totalRequiredWidth = totalPreferredWidth + Spacing.X * (children.Count - 1);
		float availableWidth = innerWidth - totalRequiredWidth;

		if (ChildForceExpandWidth && availableWidth > 0)
		{
			float expandAmount = availableWidth / children.Count;
			for (int i = 0; i < children.Count; i++)
			{
				childWidths[i] += expandAmount;
			}
			totalRequiredWidth = innerWidth; 
		}

		float startX = PaddingLeft + GetStartOffset(UIAxis.Horizontal, innerWidth, totalRequiredWidth, ChildAlignment);
		for (int i = 0; i < children.Count; i++)
		{
			var child = children[i];
			
			child.AnchorMin = child.AnchorMin with {X = 0};
			child.AnchorMax = child.AnchorMax with {X = 0};
			child.OffsetMin = child.OffsetMin with {X = startX};
			child.OffsetMax = child.OffsetMax with {X = startX + childWidths[i]};

			startX += childWidths[i] + Spacing.X;
		}
	}

	protected override void RecalculateVertical()
	{
		var children = UILayout.Children.OfType<RUILayout>().ToList();
		if (children.Count == 0) return;

		float innerHeight = UILayout.Rect.Height - PaddingTop - PaddingBottom;

		foreach (var child in children)
		{
			GetChildPreferredSize(child, out _, out float prefH);

			float height = child.Size.Y;
			if (ControlChildHeight)
			{
				height = ChildForceExpandHeight ? innerHeight : prefH;
			}

			float startY = PaddingTop + GetStartOffset(UIAxis.Vertical, innerHeight, height, ChildAlignment);
			
			child.AnchorMin = child.AnchorMin with {Y = 0};
			child.AnchorMax = child.AnchorMax with {Y = 0};
			child.OffsetMin = child.OffsetMin with {Y = startY};
			child.OffsetMax = child.OffsetMax with {Y = startY + height};
		}
	}
}