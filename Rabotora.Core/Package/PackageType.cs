namespace Rabotora.Core.Package
{
	/// <summary>
	/// Represents a type of the <see cref="RabotoraDataPack"/> .
	/// </summary>
	public enum PackageType
	{
		/// <summary>
		/// System Data Pack.
		/// <para>
		/// Common Content: Scripts, UI Layouts (with layout images), FX Sounds
		/// </para>
		/// <para>
		/// Common File Name: system.rpkg
		/// </para>
		/// </summary>
		SystemPack = 0,

		/// <summary>
		/// BGM(Background Music) Pack.
		/// <para>
		/// Common Content: BGMs, Opening Music/Ending Musics
		/// </para>
		/// <para>
		/// Common File Name: bgm.rpkg
		/// </para>
		/// </summary>
		BGMPack = 1,

		/// <summary>
		/// Background Image Pack.
		/// <para>
		/// Common Content: <i>&lt;Usually no other type of data&gt;</i>
		/// </para>
		/// <para>
		/// Common File Name: bg.rpkg
		/// </para>
		/// </summary>
		BackgroundPack = 2,

		/// <summary>
		/// CG(Computer Graphics) Image / Animation Group Data Pack.
		/// <para>
		/// Common Content: Normal CG Images, CG Thumbnails, Special CG Animation Groups<i>(such as 'HScene' CG Animation Group)</i>
		/// </para>
		/// <para>
		/// Common File Name: ev.rpkg (ev means 'Embedded video')
		/// </para>
		/// </summary>
		CGPack = 3,

		/// <summary>
		/// Character Stand Image Data Pack.
		/// <para>
		/// Common Content: Character Stand Images, Cartoon Symbol Texture Images
		/// </para>
		/// <para>
		/// Common File Name: st.rpkg
		/// </para>
		/// </summary>
		StandPack = 4,

		/// <summary>
		/// Small Dialog Image Data Pack.
		/// <para>
		/// Common Content: <i>&lt;Usually no other type of data&gt;</i>
		/// </para>
		/// <para>
		/// Common File Name: sd.rpkg
		/// </para>
		/// </summary>
		SDPack = 5,

		/// <summary>
		/// Character Voice Audio Data Pack.
		/// <para>
		/// Common Content: Character Voice Audios, System Voice Audios
		/// </para>
		/// <para>
		/// Common File Name: voice.rpkg
		/// </para>
		/// </summary>
		VoicePack = 6,

		/// <summary>
		/// [Optional]Character Face Texure Image Data Pack.<br />
		/// <b>This type of pack data usually merged into <see cref="StandPack"/> .</b>
		/// <para>
		/// Common Content: <i>&lt;Usually no other type of data&gt;</i>
		/// </para>
		/// <para>
		/// Common File Name: em.rpkg (em means 'Emoji')
		/// </para>
		/// </summary>
		AdditionalFacePack = 10,

		/// <summary>
		/// [Optional]Movie Video Data Pack.
		/// <para>
		/// Common Content: <i>&lt;Usually no other type of data&gt;</i>
		/// </para>
		/// <para>
		/// Common File Name: movie.rpkg
		/// </para>
		/// </summary>
		AdditionalMoviePack = 11,

		/// <summary>
		/// [Optional]Game Sound FX Data Pack.<br />
		/// <b>This type of pack data usually merged into <see cref="SystemPack"/> .</b>
		/// <para>
		/// Common Content: <i>&lt;Usually no other type of data&gt;</i>
		/// </para>
		/// <para>
		/// Common File Name: etc.rpkg (etc means 'et cetera'(more))
		/// </para>
		/// </summary>
		AdditionalSoundPack = 12,

		/// <summary>
		/// [Optional]CG Animation Mask Texture Image Data Pack.<br />
		/// <b>This type of pack data usually merged into <see cref="SystemPack"/> .</b>
		/// <para>
		/// Common Content: <i>&lt;Usually no other type of data&gt;</i>
		/// </para>
		/// <para>
		/// Common File Name: mask.rpkg
		/// </para>
		/// </summary>
		AdditionalMaskPack = 13
	}
}
