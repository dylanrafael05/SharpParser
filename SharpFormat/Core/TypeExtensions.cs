using System;

namespace SharpFormat
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns whether or not a type is supported by this class.
        /// </summary>
        public static bool TypeIsSupported(Type t)
        {
            return (
                t == typeof(string) ||
                t == typeof(float) ||
                t == typeof(int) ||
                t == typeof(bool) ||
                t == typeof(object) ||
                t.IsEnum ||
                (Nullable.GetUnderlyingType(t) is var ut && ut != null && TypeIsSupported(ut))
            );
        }

        /// <summary>
        /// Get the name of the given type.
        /// Throws <see cref="Exception"/> if the provided type is unsupported.
        /// </summary>
        public static string TypeName(Type t)
        {
            if (t == typeof(string))
            {
                return "string";
            }
            else if (t == typeof(float))
            {
                return "floating-point number";
            }
            else if (t == typeof(int))
            {
                return "integer";
            }
            else if (t == typeof(bool))
            {
                return "boolean";
            }
            else if (t == typeof(object))
            {
                return "anything";
            }
            else if (t.IsEnum)
            {
                return $"enumeration '{t.Name}'";
            }
            else if (Nullable.GetUnderlyingType(t) is var ut && ut != null)
            {
                return $"nullable {TypeName(ut)}";
            }

            throw new Exception("Source code error: unsupported type.");
        }
    }
}
