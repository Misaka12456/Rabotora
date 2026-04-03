using MemoryPack;

namespace RabotoraX.Core.Serialization;

/// <summary>
/// Marks a field or property as serializable by the RBinaryFormatter (RabotoraX Stage Serializer).<br />
/// To ensure the field or property is serialized and deserialized correctly, the type of the field or property must be one of:
/// <list type="bullet">
/// <item>Primitive types (<see cref="int"/>, <see cref="float"/>, <see cref="bool"/>, etc.)</item>
/// <item>Structs that can be serialized by MemoryPack (marked with <see cref="MemoryPackableAttribute"/> and <see cref="RBinarySerializableAttribute"/> or <see cref="RBinarySerializableAttribute{T}"/>)</item>
/// <item>Reference types (<see langword="class"/> types) that are marked with <see cref="RBinarySerializableAttribute"/> or <see cref="RBinarySerializableAttribute{T}"/> and derived from <see cref="Object"/></item>
/// </list>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)] // enable pass-compilation (no CS0592) for properties, as we will error this as RAT0005 in analyzer.
public class RSerializableFieldAttribute : Attribute;

/// <summary>
/// Marks a field or property as never to be serialized by the RBinaryFormatter (RabotoraX Stage Serializer).
/// </summary>
[AttributeUsage(AttributeTargets.Field)] 
public class RNonSerializedAttribute : Attribute;