using LiveStreamingServerNet.WebRTC.Sdp.Attributes;
using LiveStreamingServerNet.WebRTC.Sdp.Attributes.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace LiveStreamingServerNet.WebRTC.Sdp.Internal
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class SdpAttributeNameAttribute : Attribute
    {
        public readonly string Name;

        public SdpAttributeNameAttribute(string name)
        {
            Name = name;
        }
    }

    internal static class SdpAttributeRegistry
    {
        private delegate ISdpAttribute? SdpAttributeFactoryDelegate(string value);

        private static readonly IDictionary<string, SdpAttributeFactoryDelegate> FactoryMethodByName;

        static SdpAttributeRegistry()
        {
            var attributes = GetSdpAttributes();

            FactoryMethodByName = attributes
                .Select(pair => (pair.Name, FactoryMethodInfo: GetReadMethod(pair.Type)))
                .Where(pair => pair.FactoryMethodInfo != null)
                .Select(pair => (pair.Name, FactoryMethod: CreateFactoryMethod(pair.FactoryMethodInfo!)))
                .ToDictionary(pair => pair.Name, pair => pair.FactoryMethod, StringComparer.OrdinalIgnoreCase);

            return;

            static (Type Type, string Name)[] GetSdpAttributes() =>
                typeof(SdpAttributeRegistry).Assembly.GetTypes()
                    .Where(type => typeof(ISdpAttribute).IsAssignableFrom(type))
                    .Where(type => type is { IsInterface: false, IsAbstract: false })
                    .Where(type => type != typeof(SdpAttribute))
                    .Select(type => (Type: type, Name: type.GetCustomAttribute<SdpAttributeNameAttribute>()?.Name))
                    .Where(pair => pair.Name != null)
                    .Select(pair => (pair.Type, pair.Name!))
                    .ToArray();

            static MethodInfo? GetReadMethod(Type type) =>
                type.GetMethod(
                    "ParseValue", BindingFlags.Public | BindingFlags.Static,
                    null, [typeof(string)], null
                );

            static SdpAttributeFactoryDelegate CreateFactoryMethod(MethodInfo methodInfo)
            {
                var valueParam = Expression.Parameter(typeof(string), "value");
                var callExpression = Expression.Call(methodInfo, valueParam);
                var castExpression = Expression.Convert(callExpression, typeof(ISdpAttribute));

                return Expression.Lambda<SdpAttributeFactoryDelegate>(
                    castExpression, valueParam
                ).Compile();
            }
        }

        public static ISdpAttribute Parse(string name, string? value)
        {
            if (FactoryMethodByName.TryGetValue(name, out var factoryMethod))
            {
                var result = factoryMethod(value ?? string.Empty);
                if (result != null)
                    return result;
            }

            return SdpAttribute.ParseValue(name, value);
        }

        public static string Write(ISdpAttribute attribute) => attribute.ToString()!;
    }
}
