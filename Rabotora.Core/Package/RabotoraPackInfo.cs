using Newtonsoft.Json;

namespace Rabotora.Core.Package
{
	public struct RabotoraPackInfo
	{
		[JsonProperty("name")]
		public string PackName { get; set; }

		[JsonProperty("version")]
		public string PackVersion { get; set; }

		[JsonProperty("author")]
		public string Author { get; set; }

		[JsonProperty("dataTable")]
		public Dictionary<string, int> DataContrastiveTable { get; set; }
	}
}
