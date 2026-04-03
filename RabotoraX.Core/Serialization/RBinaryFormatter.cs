using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MemoryPack;
using RabotoraX.Core.Cinematics;
using RabotoraX.Core.UI;

namespace RabotoraX.Core.Serialization;

/// <summary>
/// Represents the base class for all serialization transfer objects (STO) used in RabotoraX Serialization.<br />
/// <b>Note:</b> Simple transfer objects are not being a root container for data; they should not derive from this base class.
/// </summary>
[MemoryPackable]
[MemoryPackUnion(0, typeof(RStageTransferObject))]
public abstract partial class RSerializeBaseTransferObject
{
	[MemoryPackIgnore]
	public static ReadOnlySpan<byte> ExpectedMagicHeader => "RATS"u8; // "RATS" stands for "RabotoraX Serialization"

	[MemoryPackOrder(0)]
	public byte[] MagicHeader { get; set; } = ExpectedMagicHeader.ToArray();
	
	[MemoryPackOrder(1)]
	public int Version { get; set; } = RBinaryFormatter.CurrentFormatVersion;

	[MemoryPackOrder(2)]
	public abstract string STOType { get; [UsedImplicitly] set; }

	public void ValidateSTOSanity()
	{
		if (!MagicHeader.AsSpan().SequenceEqual(ExpectedMagicHeader))
		{
			throw new InvalidDataException("Invalid magic header. The serialized data may be corrupted or not created by RabotoraX.");
		}
		if (Version > RBinaryFormatter.CurrentFormatVersion)
		{
			throw new NotSupportedException($"Unsupported serialized data version: {Version}. " +
			                                $"This serialized data may be created with a newer version of RabotoraX. " +
			                                $"Please update RabotoraX to the latest version.");
		}
	}
}

/// <summary>
/// Represents a RabotoraX Serialization Transfer Object for <see cref="RStage"/>.
/// </summary>
[MemoryPackable]
public partial class RStageTransferObject : RSerializeBaseTransferObject
{
	public override string STOType { get; set; } = "RStage";

	[MemoryPackOrder(3)]
	public string Name { get; set; } = string.Empty;
	
	[MemoryPackOrder(4)]
	public int Type { get; set; }
	
	[MemoryPackOrder(5)]
	public List<RObjectTransferObject> Objects { get; set; } = [];
}

/// <summary>
/// Represents a simple transfer object for RObject and its Components, used in the serialization process.
/// </summary>
[MemoryPackable]
public partial class RObjectTransferObject
{
	[MemoryPackOrder(0)]
	public string InstanceId { get; set; } = string.Empty; // corresponding to RabotoraX.Core.Object.InstanceId (Guid)
	
	[MemoryPackOrder(1)]
	public string Name { get; set; } = string.Empty;
	
	[MemoryPackOrder(2)]
	public bool IsActive { get; set; }
	
	[MemoryPackOrder(3)]
	public string ParentId { get; set; } = string.Empty;
	
	[MemoryPackOrder(4)]
	public List<ComponentTransferObject> Components { get; set; } = [];
}

/// <summary>
/// Represents a simple transfer object for Component, used in the serialization process.
/// </summary>
[MemoryPackable]
public partial class ComponentTransferObject
{
	[MemoryPackOrder(0)]
	public string TypeName { get; set; } = string.Empty;
	
	[MemoryPackOrder(1)]
	public string InstanceId { get; set; } = string.Empty;
	
	[MemoryPackOrder(2)]
	public bool IsEnabled { get; set; }
	
	[MemoryPackOrder(3)]
	public Dictionary<string, byte[]> SerializedFields { get; set; } = []; // store serializable value fields (int, Vector3, Color, or something that can be serialized by MemoryPack as struct)

	[MemoryPackOrder(4)]
	public Dictionary<string, string> ReferenceFields { get; set; } = []; // store reference fields (RObject, Component or something that marked [RBinarySerializable]/[Serializable] and derived from RabotoraX.Core.Object) with their InstanceIds as strings
	
	[MemoryPackOrder(5)]
	public Dictionary<string, List<string>> ReferenceSetFields { get; set; } = [];

	[MemoryPackOrder(6)]
	public Dictionary<string, Dictionary<string, string>> ReferenceMapFields { get; set; } = [];
}

internal class DeserializationContext : IDisposable
{
	[UsedImplicitly] public readonly List<Component> AllComponents = [];
	private readonly Dictionary<string, Object> _mapInstances = [];
	private readonly List<Action> _postResolveActions = [];

	public void RegisterInstance(string id, Object obj)
	{
		if (!_mapInstances.TryAdd(id, obj))
		{
#if !RABOTORA_STRICT || DEBUG
			throw new InvalidDataException($"Duplicate instance ID detected: {id}. The stage or prefab data may be corrupted.");
#else
			throw new InvalidDataException("Duplicate instance ID detected. The stage or prefab data may be corrupted.");
#endif
		}
		_mapInstances[id] = obj;
	}
	
	public bool TryGetInstance(string id, out Object? obj)
	{
		return _mapInstances.TryGetValue(id, out obj!);
	}

	public Object GetInstance(string id)
	{
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (!_mapInstances.TryGetValue(id, out var obj))
		{
#if !RABOTORA_STRICT || DEBUG
			throw new InvalidDataException($"Failed to resolve reference. Target instance ID '{id}' was not found.");
#else 
			throw new InvalidDataException("Failed to resolve reference. Target instance ID was not found.");
#endif
		}
		return obj;
	}
	
	public void AddPostResolveAction(Action action)
	{
		_postResolveActions.Add(action);
	}

	public void ResolveAllReferences()
	{
		foreach (var action in _postResolveActions)
		{
			action();
		}
	}

	public void Dispose()
	{
		AllComponents.Clear();
		_mapInstances.Clear();
		_postResolveActions.Clear();
	}
}

/// <summary>
/// Represents a binary formatter for RabotoraX Serialization.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class RBinaryFormatter : IDisposable, IAsyncDisposable
{
	public const int CurrentFormatVersion = 1;
	
	private Stream? _stream;

	public RBinaryFormatter(Stream stream)
	{
		_stream = stream;
	}

	[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Fields are guaranteed by Source Generator")]
	public async ValueTask SerializeAsync(RStage stage)
	{
		if (_stream is not {CanWrite: true}) throw new InvalidOperationException("Stream is not writable.");

		var dto = new RStageTransferObject()
		{
			Version = CurrentFormatVersion,
			Name = stage.Name,
			Type = (int) stage.Type,
		};
		
		var allObjects = new List<RObject>();
		CollectAllObjects(stage.RootObjects, allObjects);

		foreach (var managedObj in allObjects)
		{
			var objDto = new RObjectTransferObject()
			{
				InstanceId = managedObj.InstanceId,
				Name = managedObj.Name,
				IsActive = managedObj.IsActive,
				ParentId = managedObj.Layout.Parent?.RObject.InstanceId ?? string.Empty
			};

			foreach (var comp in managedObj.EnumerateComponents(includeInactive: true))
			{
				var compDto = new ComponentTransferObject()
				{
					TypeName = comp.GetType().FullName ?? comp.GetType().Name,
					InstanceId = comp.InstanceId,
					IsEnabled = comp.IsEnabled
				};
				
				var fields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(Serializable);

				foreach (var field in fields)
				{
					object? val = field.GetValue(comp);
					if (val == null) continue;

					ProcessAndSerializeMember(field.FieldType, field.Name, val,
						compDto.SerializedFields, compDto.ReferenceFields,
						compDto.ReferenceSetFields, compDto.ReferenceMapFields);
				}
				
				objDto.Components.Add(compDto);
			}
			
			dto.Objects.Add(objDto);
		}
		
		await MemoryPackSerializer.SerializeAsync<RSerializeBaseTransferObject>(_stream, dto);
	}

	public async ValueTask<T> DeserializeAsync<T>() where T : Object
	{
		if (_stream is not {CanRead: true}) throw new InvalidOperationException("Stream is not readable.");

		return typeof(T) switch
		{
			{ } t when t == typeof(RStage) => (T)(Object) await DeserializeStageAsync(),
			_ => throw new NotSupportedException($"Deserialization of type {typeof(T).FullName} is not supported.")
		};
	}

	[UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "AOT SourceGen guarantees type preservations")]
	[UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "AOT SourceGen guarantees type preservations")]
	[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "AOT SourceGen guarantees type preservations")]
	private async ValueTask<RStage> DeserializeStageAsync()
	{
		// because we have marked MemoryPackUnion, it will find and correctly deserialized to RStageTransferObject based on RSerializeBaseTransferObject.
		if (await MemoryPackSerializer.DeserializeAsync<RSerializeBaseTransferObject>(_stream!) is not RStageTransferObject dto)
		{
			throw new InvalidDataException("Failed to deserialize stage data.");
		}
		dto.ValidateSTOSanity();

		var stage = new RStage(dto.Name)
		{
			Type = (StageType)dto.Type
		};
		using var context = new DeserializationContext();
		var parentMapping = new Dictionary<string, string>(); // (ChildInstanceId, ParentInstanceId)

		foreach (var objDto in dto.Objects)
		{
			var ro = new RObject(objDto.Name)
			{
				Stage = stage,
				InstanceId = objDto.InstanceId,
				IsActive = objDto.IsActive
			};
			
			context.RegisterInstance(ro.InstanceId, ro);

			if (!string.IsNullOrEmpty(objDto.ParentId))
			{
				parentMapping[objDto.InstanceId] = objDto.ParentId;
			}

			stage.AddUninitializedObject(ro); // add the object, whatever it has a parent or not (equals to RStage.CreateObject)

			foreach (var compDto in objDto.Components)
			{
				Component? comp = null;

				if (compDto.TypeName == typeof(RLayout).FullName || compDto.TypeName == typeof(RUILayout).FullName)
				{
					if (compDto.TypeName == typeof(RUILayout).FullName && ro.Layout is not RUILayout)
					{
						ro.AddComponent<RUILayout>();
					}
					comp = ro.Layout;
					comp.InstanceId = compDto.InstanceId;
					comp.IsEnabled = compDto.IsEnabled;
				}
				else if (RTypeRegistry.Types.TryGetValue(compDto.TypeName, out var compType))
				{
					if (RTypeRegistry.Factories.TryGetValue(compDto.TypeName, out var factory) && factory != null)
					{
						comp = (Component)factory()!; // Call Source Generator provided factory method! 100% Native AOT guaranteed safe!
					}
					else
					{
						comp = (Component)Activator.CreateInstance(compType)!; // Fallback
					}
					comp.InstanceId = compDto.InstanceId;
					comp.IsEnabled = compDto.IsEnabled;
					
					ro.UnsafeAddUninitializedComponent(comp);
				}

				if (comp != null)
				{
					context.RegisterInstance(comp.InstanceId, comp);
					if (comp is not RLayout) context.AllComponents.Add(comp); // we will call OnAwake and OnStart for non-layout components after resolving all references
					var compTypeInfo = comp.GetType();
					
					RestoreMemberData(comp, compTypeInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
						context, compDto.SerializedFields, compDto.ReferenceFields, compDto.ReferenceSetFields, compDto.ReferenceMapFields);
				}
			}
		}

		foreach (var kvp in parentMapping)
		{
			if (context.TryGetInstance(kvp.Key, out var childObj) && childObj is RObject child &&
			    context.TryGetInstance(kvp.Value, out var parentObj) && parentObj is RObject parent)
			{
				child.Layout.SetParent(parent.Layout);
			}
		}
		
		context.ResolveAllReferences();

		// foreach (var comp in context.AllComponents)
		// {
		// 	comp.OnAwake();
		// }
		// OnAwake and OnStart will be invoked when loading the RStage by the Cinema (Stage Manager), so we don't need to call them here.
		
		return stage;
	}
	
	public void Flush()
	{
		_stream?.Flush();
	}
	
	public void Close()
	{
		_stream?.Flush();
		_stream?.Close();
	}
	
	public void Dispose()
	{
		_stream?.Dispose();
		_stream = null;
		GC.SuppressFinalize(this);
	}

	public async ValueTask DisposeAsync()
	{
		if (_stream != null)
		{
			await _stream.DisposeAsync();
			_stream = null;
		}
		GC.SuppressFinalize(this);
	}

	#region Utility Methods for Serialization and Deserialization
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ProcessAndSerializeMember(Type type, string name, object val, Dictionary<string, byte[]> serializedMap, Dictionary<string, string> refMap,
		Dictionary<string, List<string>> refSetMap, Dictionary<string, Dictionary<string, string>> refMapMap)
	{
		if (typeof(Object).IsAssignableFrom(type))
		{
			refMap[name] = ((Object)val).InstanceId;
		}
		else if (IsObjectSet(type, out _))
		{
			var list = new List<string>();
			foreach (var item in (IEnumerable) val)
			{
				if (item is Object o) list.Add(o.InstanceId);
			}
			refSetMap[name] = list;
		}
		else if (IsObjectMap(type, out _, out _))
		{
			var dict = new Dictionary<string, string>();
			if (val is IDictionary iDict)
			{
				foreach (DictionaryEntry kvp in iDict)
				{
					if (kvp.Value is Object o) dict[kvp.Key.ToString() ?? string.Empty] = o.InstanceId;
				}
			}
			refMapMap[name] = dict;
		}
		else
		{
			serializedMap[name] = MemoryPackSerializer.Serialize(type, val);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Types are instantiated safely")]
	[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Methods are retrieved and invoked safely")]
	private static void RestoreMemberData(Component comp, IEnumerable<MemberInfo> members, DeserializationContext context,
		Dictionary<string, byte[]> serializedMap, Dictionary<string, string> refMap, Dictionary<string, List<string>> refSetMap, Dictionary<string, Dictionary<string, string>> refMapMap)
	{
		foreach (var member in members)
		{
			bool isField = member is FieldInfo;
			var fieldInfo = member as FieldInfo;
			if (!isField) continue;
			
			var memberType = fieldInfo!.FieldType;
			
			if (fieldInfo.IsInitOnly) continue; // skip readonly fields

			// ReSharper disable once ConvertToLocalFunction
			Action<object?> setValue = val =>
			{
				fieldInfo.SetValue(comp, val);
			};

			if (serializedMap.TryGetValue(member.Name, out byte[]? bytes))
			{
				object? val = MemoryPackSerializer.Deserialize(memberType, bytes);
				setValue(val);
			}
			else if (refMap.TryGetValue(member.Name, out string? targetId))
			{
				context.AddPostResolveAction(() =>
				{
					try
					{
						var instance = context.GetInstance(targetId);
						setValue(instance);
					}
					catch
					{
#if DEBUG
						Console.WriteLine("[RBinaryFormatter] Failed to resolve reference for member '{0}' with target instance ID '{1}'. The object reference may be dynamically created.", member.Name, targetId);
#endif
					}
				});
			}
			else if (refSetMap.TryGetValue(member.Name, out var idList) && IsObjectSet(memberType, out var elementType))
			{
				context.AddPostResolveAction(() =>
				{
					if (memberType.IsArray)
					{
						var array = Array.CreateInstance(elementType!, idList.Count);
						for (int i = 0; i < idList.Count; i++)
						{
							array.SetValue(context.GetInstance(idList[i]), i);
						}
						setValue(array);
					}
					else
					{
						var collection = Activator.CreateInstance(memberType);
						if (collection is IList list)
						{
							foreach (string id in idList)
							{
								list.Add(context.GetInstance(id));
							}
						}
						else // regular IEnumerable collection with Add method (like HashSet<T>)
						{
							var addMethod = memberType.GetMethod("Add");
							foreach (string id in idList)
							{
								addMethod?.Invoke(collection, [context.GetInstance(id)]);
							}
							setValue(collection);
						}
					}
				});
			}
			else if (refMapMap.TryGetValue(member.Name, out var dictPairs) && IsObjectMap(memberType, out var keyType, out _))
			{
				context.AddPostResolveAction(() =>
				{
					var dict = (IDictionary)Activator.CreateInstance(memberType)!;
					foreach (var pair in dictPairs) // regular IDictionary (IDictionary must have Add method and support enumeration of DictionaryEntry)
					{
						object parsedKey = keyType!.IsEnum ? Enum.Parse(keyType, pair.Key) : Convert.ChangeType(pair.Key, keyType);
						dict.Add(parsedKey, context.GetInstance(pair.Value));
					}
					setValue(dict);
				});
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CollectAllObjects(IEnumerable<RObject> currentLevel, List<RObject> result)
	{
		var stack = new Stack<RObject>(currentLevel.Reverse());
		var visited = new HashSet<RObject>(RObject.EqualityComparer);
		while (stack.Count > 0)
		{
			var ro = stack.Pop();
			if (!visited.Add(ro)) continue;
			result.Add(ro);
			var children = ro.Layout.Children.Select(c => c.RObject);
			foreach (var child in children.Reverse())
			{
				stack.Push(child);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool Serializable(FieldInfo f)
	{
		bool isPublicInstance = f.IsPublic;
		bool markedSerializableField = f.GetCustomAttribute<RSerializableFieldAttribute>() != null;
		// auto-property backing fields are not serialized even if they are public, unless they are explicitly marked with [RSerializableField]
		bool isAutoPropBackingField = (f.Name.Contains("k__BackingField") && f.GetCustomAttribute<CompilerGeneratedAttribute>() != null) || f.Name.Contains('<') || f.Name.Contains('>');
		if (isAutoPropBackingField && markedSerializableField) return true; // allow explicitly marked auto-property backing fields as [field: RSerializableField]
		
		bool canSerializeCondition = isPublicInstance || markedSerializableField;
		if (!canSerializeCondition) return false;
		
		bool hasNonSerialized = f.GetCustomAttribute<NonSerializedAttribute>() != null ||
		                        f.GetCustomAttribute<RNonSerializedAttribute>() != null ||
		                        f.GetCustomAttribute<MemoryPackIgnoreAttribute>() != null;
		if (hasNonSerialized) return false;
		
		var fieldType = f.FieldType;
		bool isBlittable = IsBlittable(fieldType);
		bool isValueType = fieldType.IsValueType;
		bool isString = fieldType == typeof(string);
		bool hasManagedBinarySerializable = Attribute.IsDefined(f, typeof(RBinarySerializableAttribute));
		bool hasSerializable = f.GetCustomAttribute<SerializableAttribute>() != null;
		
		bool isObjectReference = typeof(Object).IsAssignableFrom(fieldType);
		bool isObjectCollection = IsObjectSet(fieldType, out _) || IsObjectMap(fieldType, out _, out _);
		
		bool isExcluded = !isBlittable && !isValueType && !isString && !isObjectReference && !isObjectCollection && !hasManagedBinarySerializable && !hasSerializable;
		
		return !isExcluded;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsBlittable(Type type)
	{
		if (type.IsPrimitive) return true; // primitive types (int, float, bool, etc.) are blittable (some platforms may treat bool as non-blittable, but MemoryPack will handle this)
		if (type == typeof(decimal)) return false; // decimal is not blittable
		if (type == typeof(nint) || type == typeof(nuint)) return true; // nint (IntPtr) and nuint (UIntPtr) are blittable
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Interfaces are statically guaranteed")]
	private static bool IsObjectSet(Type type, out Type? elementType)
	{
		elementType = null;
		if (type.IsArray)
		{
			elementType = type.GetElementType();
			return typeof(Object).IsAssignableFrom(elementType);
		}

		if (type.IsGenericType)
		{
			var genDef = type.GetGenericTypeDefinition();
			if (genDef == typeof(List<>) || genDef == typeof(HashSet<>))
			{
				elementType = type.GetGenericArguments()[0];
				return typeof(Object).IsAssignableFrom(elementType);
			}
		}
		
		if (type.IsAssignableFrom(typeof(IEnumerable)))
		{
			var iEnumerableType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			if (iEnumerableType != null)
			{
				elementType = iEnumerableType.GetGenericArguments()[0];
			}
			elementType ??= typeof(object);
			return typeof(Object).IsAssignableFrom(elementType);
		}
		
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Interfaces are statically guaranteed")]
	private static bool IsObjectMap(Type type, out Type? keyType, [UsedImplicitly] out Type? valueType)
	{
		keyType = null;
		valueType = null;
		if (type.IsGenericType)
		{
			if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			{
				var args = type.GetGenericArguments();
				keyType = args[0];
				valueType = args[1];
				return typeof(Object).IsAssignableFrom(valueType);
			}
			if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
			{
				var dictInterface = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
				var args = dictInterface.GetGenericArguments();
				keyType = args[0];
				valueType = args[1];
				return typeof(Object).IsAssignableFrom(valueType);
			}
		}
		return false;
	}
	#endregion
}