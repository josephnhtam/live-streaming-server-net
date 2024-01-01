namespace LiveStreamingServerNet.Utilities.Extensions
{
    public static class TypeExtensions
    {
        public static Type[] GetGenericArguments(this Type type, Type genericType)
        {
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
