namespace RabotoraX.Core.Serialization;

/// <summary>
/// Represents the central registry for types that can be serialized/deserialized by RabotoraX's serialization system.
/// </summary>
public static class RTypeRegistry
{
	/// <summary>
	/// Types registered for serialization/deserialization, mapped by their unique string keys.
	/// </summary>
	public readonly static Dictionary<string, Type> Types = [];
	
	/// <summary>
	/// Optional factory functions for creating instances of registered types during deserialization, mapped by the same unique string keys.
	/// </summary>
	public readonly static Dictionary<string, Func<object?>?> Factories = [];
	
	/// <summary>
	/// Explicitly registers a type with the registry for Native AOT type-searching compatibility.
	/// </summary>
	/// <param name="name">The unique name to associate with the type. This should typically be the full name of the type, including its namespace, to avoid conflicts.</param>
	/// <param name="type">The actual <see cref="Type"/> object representing the type to be registered.</param>
	/// <param name="factory">An optional factory function that can be used to create instances of the type during deserialization.
	/// If null, the system will attempt to use a parameterless constructor or other means to instantiate the type, unless the type is abstract or an interface, in which case it will not be instantiated directly.</param>
	public static void Register(string name, Type type, Func<object?>? factory)
	{
		Types[name] = type;
		Factories[name] = factory;
	}
}