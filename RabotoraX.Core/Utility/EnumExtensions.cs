using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RabotoraX.Core.Utility;

public static class EnumExtensions
{
	private readonly static Dictionary<Enum, string> Cache = [];

	public static string GetDescription<T>(this T enumValue) where T : Enum
	{
		if (Cache.TryGetValue(enumValue, out string? cached))
		{
			return cached;
		}

		string? name = Enum.GetName(typeof(T), enumValue);
		var field = typeof(T).GetField(name!);
		var attr = field?.GetCustomAttribute<DescriptionAttribute>();

		string result = attr?.Description ?? name ?? enumValue.ToString();
		Cache[enumValue] = result;

		return result;
	}
}