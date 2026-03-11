namespace RabotoraX.Core.Graphics;

public struct InputElementDescription
{
	public string SemanticName; // "POSITION", "TEXCOORD", etc.
	public int SemanticIndex;
	public int FormatSize;
	public int Offset;
	
	public InputElementDescription(string name, int index, int formatSize, int offset)
	{
		SemanticName = name;
		SemanticIndex = index;
		FormatSize = formatSize;
		Offset = offset;
	}
}