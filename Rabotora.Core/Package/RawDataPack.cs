#pragma warning disable 8618
using FlatSharp.Attributes;

namespace Rabotora.Core.Package
{
	[FlatBufferTable]
	internal class RawDataPack
	{
		/// <summary>
		/// The metadata information of current Rabotora Raw Data Pack.
		/// </summary>
		[FlatBufferItem(0)]
		public virtual byte[] MetaInfoData { get; set; }

		/// <summary>
		/// The main data list part of current Rabotora Raw Data Pack.
		/// <para>
		/// Structure: <br />
		/// <code>{Index1}{Length1}{Data1...}{Index2}{Length2}{Data2...}...</code><br />
		/// The type of Index is <see cref="int"/> (4-bytes Length);<br />
		/// the type of Length is <see cref="long"/> (8-bytes Length).
		/// </para>
		/// </summary>
		[FlatBufferItem(1)]
		public virtual byte[] DataList { get; set; }
	}
}
