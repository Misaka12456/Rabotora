using Newtonsoft.Json;

namespace Rabotora.Core
{
	/// <summary>
	/// Represents a series of information for a Rabotora Game Project.
	/// </summary>
	public struct RabotoraGameProjectInfo
	{
		/// <summary>
		/// The name of the game, usually contains ASCII characters.
		/// </summary>
		[JsonProperty("name")]
		public string GameName { get; set; } = string.Empty;

		/// <summary>
		/// The title of the game, usually contains Unicode characters.
		/// </summary>
		[JsonProperty("title")]
		public string GameTitle { get; set; } = string.Empty;

		/// <summary>
		/// The version of the game.
		/// </summary>
		[JsonProperty("version")]
		public string GameVersion { get; set; } = string.Empty;

		/// <summary>
		/// The publish type of the game.
		/// </summary>
		[JsonProperty("type")]
		public RabotoraGameType GameType { get; set; } = RabotoraGameType.StandaloneDiscGame;

		/// <summary>
		/// The company who made the game.
		/// </summary>
		[JsonProperty("company")]
		public string Company { get; set; } = string.Empty;

		/// <summary>
		/// The copyright declaration of the game.
		/// </summary>
		[JsonProperty("copyright")]
		public string Copyright { get; set; } = string.Empty;

		/// <summary>
		/// Is the game a restricted level game?
		/// </summary>
		public bool IsRestricted { get; set; } = false;
	}
}
