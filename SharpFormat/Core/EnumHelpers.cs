using System;
using System.Reflection;
using System.Linq;

namespace SharpFormat
{
    /// <summary>
    /// Holds extensions and helpers for the enums.
    /// </summary>
    public static class EnumHelpers
    {
        /// <summary>
        /// Holds a MethodInfo corresponding to <see cref="Enum.TryParse{TEnum}(string, out TEnum)"/>.
        /// </summary>
        public static MethodInfo TryParseStrGenericDefMInfo { get; } =

            typeof(Enum)
            .GetMethods(BindingFlags.Public)
            .First(
                m => m.Name == "TryParse"
                    && m.IsGenericMethod
                    && m.GetParameters() is var parameters
                    && parameters[0].ParameterType == typeof(string)
                    && parameters[1].IsOut
            )

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
            var ret = TryParseStrGenericDefMInfo.MakeGenericMethod(t).Invoke(null, parameters);
            value = parameters[1];

            return (bool)ret;
#endif
        }
    }
}
