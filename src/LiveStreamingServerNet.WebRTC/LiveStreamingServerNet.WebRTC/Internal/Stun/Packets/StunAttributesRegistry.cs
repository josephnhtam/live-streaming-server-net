using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal class StunAttributesRegistry
    {
        private delegate IStunAttribute StunAttributeFactoryDelegate(IDataBuffer buffer, ushort length);

        private static readonly IDictionary<ushort, StunAttributeFactoryDelegate> FactoryMethodByType;

        static StunAttributesRegistry()
        {
            var attributes = GetStunAttributes();

            FactoryMethodByType = attributes
                .Select(pair => (pair.Id, FactoryMethodInfo: GetReadMethod(pair.Type)))
                .Where(pair => pair.FactoryMethodInfo != null)
                .Select(pair => (pair.Id, FactoryMethod: CreateFactoryMethod(pair.FactoryMethodInfo!)))
                .ToFrozenDictionary(pair => pair.Id, pair => pair.FactoryMethod);

            return;

            static (Type Type, ushort Id)[] GetStunAttributes() =>
                typeof(StunAttributesRegistry).Assembly.GetTypes()
                    .Where(type => typeof(IStunAttribute).IsAssignableFrom(type))
                    .Where(type => type is { IsInterface: false, IsAbstract: false })
                    .Select(type => (Type: type, Id: type.GetCustomAttribute<StunAttributeTypeAttribute>()?.Type))
                    .Where(pair => pair.Id.HasValue)
                    .Select(pair => (pair.Type, pair.Id!.Value))
                    .ToArray();

            static MethodInfo? GetReadMethod(Type type) =>
                type.GetMethod(
                    "ReadValue", BindingFlags.Public | BindingFlags.Static,
                    null, [typeof(IDataBuffer), typeof(ushort)], null
                );

            static StunAttributeFactoryDelegate CreateFactoryMethod(MethodInfo methodInfo)
            {
                var bufferParam = Expression.Parameter(typeof(IDataBuffer), "buffer");
                var lengthParam = Expression.Parameter(typeof(ushort), "length");

                var callExpression = Expression.Call(methodInfo, bufferParam, lengthParam);
                var castExpression = Expression.Convert(callExpression, typeof(IStunAttribute));

                return Expression.Lambda<StunAttributeFactoryDelegate>(
                    castExpression, bufferParam, lengthParam
                ).Compile();
            }
        }

        public static IStunAttribute? ReadAttributeValue(ushort type, ushort length, IDataBuffer buffer) =>
            FactoryMethodByType.TryGetValue(type, out var factoryMethod) ? factoryMethod(buffer, length) : null;
    }
}
