namespace LiveStreamingServerNet.Utilities.Extensions
{
    public static class TypeExtensions
    {
        public static Type[] GetGenericArguments(this Type type, Type genericType)
        {
            if (genericType.IsInterface)
            {
                return type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                    ?.GetGenericArguments() ??
                    throw new ArgumentException("Failed to resolve generic arguments");
            }

            Type current = type;

            while (!current.IsGenericType || current.GetGenericTypeDefinition() != genericType)
            {
                if (current.BaseType != null)
                {
                    current = current.BaseType;
                }
                else
                {
                    throw new ArgumentException("Failed to resolve generic arguments");
                }
            }

            return current.GetGenericArguments();
        }

        public static bool CheckGenericTypeDefinition(this Type type, Type genericType)
        {
            if (genericType.IsInterface)
            {
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType);
            }

            Type current = type;

            while (!current.IsGenericType || current.GetGenericTypeDefinition() != genericType)
            {
                if (current.BaseType != null)
                {
                    current = current.BaseType;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
