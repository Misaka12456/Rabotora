namespace RabotoraX.Core.Graphics;

public struct InputElementDescription
{
	public string SemanticName; // "POSITION", "TEXCOORD", etc.
	public int SemanticIndex;
	public GpuFormat Format;
	public int AlignedByteOffset; // Offset in bytes from the start of the vertex
	public int InputSlot; // For future use (e.g., multiple vertex buffers)
	
	public InputElementDescription(string name, int index, GpuFormat format, int alignedByteOffset, int inputSlot)
	{
		SemanticName = name;
		SemanticIndex = index;
		Format = format;
		AlignedByteOffset = alignedByteOffset;
		InputSlot = inputSlot;
	}
}