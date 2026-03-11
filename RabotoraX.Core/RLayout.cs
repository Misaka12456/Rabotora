using System.Numerics;

namespace RabotoraX.Core;

public class RLayout : Component
{
	/// <summary>
	/// Represents the layout's local position.
	/// </summary>
	public Vector3 Position { get; set; }
	public Quaternion Rotation { get; set; } = Quaternion.Identity;
	public Vector3 Scale { get; set; } = Vector3.One;
	public RLayout? Parent { get; internal set; }
	public Matrix4x4 WorldMatrix => CalculateWorldMatrix();
	private readonly List<RLayout> _children = [];
	public IReadOnlyList<RLayout> Children => _children;

	public void SetParent(RLayout? parent, bool preserveWorldLayout = false)
	{
		if (Parent == parent) return;
		
		var oldWorldMatrix = WorldMatrix;
		
		Parent?._children.Remove(this);
		Parent = parent;
		Parent?._children.Add(this);

		if (!preserveWorldLayout) return;
		
		var newParentWorldMatrix = Parent?.WorldMatrix ?? Matrix4x4.Identity;

		if (Matrix4x4.Invert(newParentWorldMatrix, out var parentInverse))
		{
			var newLocalMatrix = oldWorldMatrix * parentInverse;
			if (Matrix4x4.Decompose(newLocalMatrix, out var newScale, out var newRotation, out var newTranslation))
			{
				Scale = newScale;
				Rotation = newRotation;
				Position = newTranslation;
			}
		}
		else
		{
			Scale = Vector3.One;
			Rotation = Quaternion.Identity;
			Position = oldWorldMatrix.Translation;
		}
	}

	private Matrix4x4 CalculateWorldMatrix()
	{
		var local = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
		if (Parent == null) return local;
		return local * Parent.CalculateWorldMatrix();
	}
}