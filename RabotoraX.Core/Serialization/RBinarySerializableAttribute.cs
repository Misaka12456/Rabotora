namespace RabotoraX.Core.Serialization;

/// <summary>
/// Marks a class or struct as serializable by the RBinaryFormatter (RabotoraX Stage Serializer).<br />
/// See https://docs.misakacastle.moe/rabotora/serialization for more details about RabotoraX Stage Serialization and how to use it.
/// </summary>
/// <seealso cref="RBinarySerializableAttribute{T}"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)] // Allows derived classes (Inherited == true) to be marked as serializable if the base class is marked with this attribute to enable recursive (hierarchical) serialization
public class RBinarySerializableAttribute : Attribute;

/// <summary>
/// Marks a generic class or struct as serializable by the RBinaryFormatter (RabotoraX Stage Serializer).<br />
/// This will register the use of the generic type with the class or struct to make sure generic types can be serialized and deserialized correctly (especially in AOT (Ahead-of-Time) environments where reflection is limited).<br />
/// See https://docs.misakacastle.moe/rabotora/serialization for more details about RabotoraX Stage Serialization and how to use it.
/// </summary>
/// <typeparam name="T">
/// The generic type parameter to register for serialization and to preserve from trimming.<br />
/// For example, to register a generic class like <c>MyComponent&lt;T&gt;</c> in runtime as the following:
/// <code>
/// var myIntComponent = GetComponent&lt;MyComponent&lt;int&gt;&gt;();
/// </code>
/// Use the following way to register the generic type with the class:
/// <code>
/// [RBinarySerializable&lt;int&gt;]
/// public class MyComponent&lt;T&gt; : RabotoraX.Core.Component
/// </code>
/// </typeparam>
/// <seealso cref="RBinarySerializableAttribute"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)] // Allows derived classes (Inherited == true) to be marked as serializable if the base class is marked with this attribute to enable recursive (hierarchical) serialization
public class RBinarySerializableAttribute<T> : RBinarySerializableAttribute;