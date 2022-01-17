using System;
using System.Reflection;
using System.Linq;

namespace SharpParser.Helpers
{
    /// <summary>
    /// Holds extensions and helpers for the enums.
    /// </summary>
    public static class EnumHelpers
    {
        /// <summary>
        /// Holds a MethodInfo corresponding to <see cref="Enum.TryParse{TEnum}(string, out TEnum)"/>.
        /// </summary>
        private static MethodInfo TryParseGen =
            typeof(Enum)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "TryParse" && m.GetParameters().Length == 2 && m.GetGenericArguments().Length == 1)
        ;

        /// <summary>
        /// Try to parse an enum value from a given enum type.
        /// Works with or without .NET Standard 2.1 or greater
        /// </summary>
        public static bool TryParseFromType(Type t, string s, out object value)
        {
#if NETSTANDARD2_1_OR_GREATER
            return Enum.TryParse(t, s, out value);
#else
            var parameters = new object[] { s, Activator.CreateInstance(t) };
            var ret = TryParseGen.MakeGenericMethod(t).Invoke(null, parameters);
            value = parameters[1];

            return (bool)ret;
#endif
        }
    }
}
