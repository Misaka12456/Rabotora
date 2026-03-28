namespace RabotoraX.Core.UI;

/// <summary>
/// Represents a layout group that arranges its child elements vertically, with options for spacing, padding, alignment, and control over child sizes.
/// </summary>
public class UIVerticalLayoutGroup : UILayoutGroup
{
	protected override void RecalculateHorizontal()
	{
		var children = UILayout.Children.OfType<RUILayout>().ToList();
		if (children.Count == 0) return;

		float innerWidth = UILayout.Rect.Width - PaddingLeft - PaddingRight;

		foreach (var child in children)
		{
			GetChildPreferredSize(child, out float prefW, out _);

			float width = child.Size.X;
			if (ControlChildWidth)
			{
				width = ChildForceExpandWidth ? innerWidth : prefW;
			}

			float startX = PaddingLeft + GetStartOffset(UIAxis.Horizontal, innerWidth, width, ChildAlignment);
			
			child.AnchorMin = child.AnchorMin with {X = 0};
			child.AnchorMax = child.AnchorMax with {X = 0};
			child.OffsetMin = child.OffsetMin with {X = startX};
			child.OffsetMax = child.OffsetMax with {X = startX + width};
		}
	}

	protected override void RecalculateVertical()
	{
		var children = UILayout.Children.OfType<RUILayout>().ToList();
		if (children.Count == 0) return;

		float innerHeight = UILayout.Rect.Height - PaddingTop - PaddingBottom;
		float[] childHeights = new float[children.Count];
		float totalPreferredHeight = 0;

		for (int i = 0; i < children.Count; i++)
		{
			GetChildPreferredSize(children[i], out _, out float prefH);
			float height = ControlChildHeight ? prefH : children[i].Size.Y;
			childHeights[i] = height;
			totalPreferredHeight += height;
		}

		float totalRequiredHeight = totalPreferredHeight + Spacing.Y * (children.Count - 1);
		float availableHeight = innerHeight - totalRequiredHeight;

		if (ChildForceExpandHeight && availableHeight > 0)
		{
			float expandAmount = availableHeight / children.Count;
			for (int i = 0; i < children.Count; i++)
			{
				childHeights[i] += expandAmount;
			}
			totalRequiredHeight = innerHeight; 
		}

		float startY = PaddingTop + GetStartOffset(UIAxis.Vertical, innerHeight, totalRequiredHeight, ChildAlignment);
		for (int i = 0; i < children.Count; i++)
		{
			var child = children[i];

			child.AnchorMin = child.AnchorMin with {Y = 0};
			child.AnchorMax = child.AnchorMax with {Y = 0};
			child.OffsetMin = child.OffsetMin with {Y = startY};
			child.OffsetMax = child.OffsetMax with {Y = startY + childHeights[i]};

			startY += childHeights[i] + Spacing.Y;
		}
	}
}