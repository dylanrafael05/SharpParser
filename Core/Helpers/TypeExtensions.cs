using System;
using System.Linq;

namespace SharpParser.Helpers
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns whether or not a type is supported by this class.
        /// </summary>
        public static bool TypeIsSupported(Type t)
        {
            return
                t == typeof(string) ||
                t == typeof(float) ||
                t == typeof(int) ||
                t == typeof(bool) ||
                t == typeof(object) ||
                t.IsEnum ||
                (Nullable.GetUnderlyingType(t) is var ut && ut != null && TypeIsSupported(ut)) ||
                (ValueTupleExtensions.UnderlyingTypes(t) is var underlyingTypes && underlyingTypes != null && underlyingTypes.All(ut1=> TypeIsSupported(ut1.Type))) ||
                (ArrayExtensions.UnderlyingType(t) is var underlyingType && underlyingType != null && TypeIsSupported(underlyingType))
            ;
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
            else if(ValueTupleExtensions.UnderlyingTypes(t) is var uts && uts != null)
            {
                if(uts.Length == 1)
                {
                    return $"tuple of {TypeName(uts[0].Type)}";
                }
                else if(uts.Length == 2)
                {
                    return $"tuple of {TypeName(uts[0].Type)} and {TypeName(uts[1].Type)}";
                }
                else
                {
                    return $"tuple of " + string.Join(", ", uts.Take(uts.Length - 1)) + $", and {uts[uts.Length - 1].Type}";
                }
            }
            else if(ArrayExtensions.UnderlyingType(t) is var underlyingType && underlyingType != null)
            {
                return $"array of {TypeName(underlyingType)}";
            }

            throw new Exception("Source code error: unsupported type.");
        }
    }
}
