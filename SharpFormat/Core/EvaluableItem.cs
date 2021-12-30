using System;
using System.Reflection;
using System.Linq;
using static SharpFormat.TypeExtensions;

namespace SharpFormat
{
    /// <summary>
    /// A class which represents an object value.
    /// </summary>
    internal class EvaluableItem
    {
        public virtual object Value { get; }

        public EvaluableItem(object value)
        {
            if (value != null)
                CheckTypeIsSupported(value.GetType());

            Value = value;
        }

        protected string InvalidTypeMessage<T>() => InvalidTypeMessage(typeof(T));
        protected string InvalidTypeMessage(Type t) => $"Invalid value for {TypeName(t)}: {Value}";


        private static void CheckTypeIsSupported(Type t)
        {
            if (!TypeIsSupported(t))
            {
                throw new Exception("Source code error: unsupported type.");
            }
        }

        public virtual object GetAsString()
        {
            if (Value is string s)
            {
                return s;
            }

            throw new Exception(InvalidTypeMessage<string>());
        }

        public virtual object GetAsInt()
        {
            if (Value is int i)
            {
                return i;
            }

            throw new Exception(InvalidTypeMessage<int>());
        }

        public virtual object GetAsFloat()
        {
            if (Value is float f)
            {
                return f;
            }
            else if (Value is int i)
            {
                return (float)i;
            }

            throw new Exception(InvalidTypeMessage<float>());
        }

        public virtual object GetAsBool()
        {
            if (Value is bool b)
            {
                return b;
            }

            throw new Exception(InvalidTypeMessage<bool>());
        }

        public virtual object GetAsEnum(Type t)
        {
            if (Value.GetType() == t)
            {
                return Value;
            }

            throw new Exception(InvalidTypeMessage(t));
        }

        public virtual object GetAsNullable(Type t, Type ut)
        {
            if(ut == typeof(int))
            {
                return Value == null ? (int?)null : GetAsInt();
            }
            else if(ut == typeof(float))
            {
                return Value == null ? (float?)null : GetAsFloat();
            }
            else if (ut == typeof(bool))
            {
                return Value == null ? (bool?)null : (bool?)GetAsBool();
            }
            else if (ut.IsEnum)
            {
                return Value == null ? Activator.CreateInstance(t) : Activator.CreateInstance(t, GetAsEnum(ut));
            }

            // SHOULD NOT HAPPEN
            throw new Exception(InvalidTypeMessage(t));
        }

        public Func<object> GetterForType(Type t)
        {
            if (t == typeof(string))
                return GetAsString;
            else if (t == typeof(int))
                return GetAsInt;
            else if (t == typeof(float))
                return GetAsFloat;
            else if (t == typeof(bool))
                return GetAsBool;
            else if (t == typeof(object))
                return () => Value;
            else if (t.IsEnum)
                return () => GetAsEnum(t);
            else if (Nullable.GetUnderlyingType(t) is var ut && ut != null)
                return () => GetAsNullable(t, ut);

            return null;
        }
    }

    /// <summary>
    /// A value stored in its literal form.
    /// </summary>
    internal class LiteralItem : EvaluableItem
    {
        public LiteralItem(object value) : base(value) { }

        public override object GetAsBool()
        {
            if (Value is string s && (s == "true" || s == "false"))
            {
                return s == "true";
            }

            throw new Exception(InvalidTypeMessage<bool>());
        }

        public override object GetAsEnum(Type t)
        {
            if (Value is string s && EnumHelpers.TryParseFromType(t, s, out var value))
            {
                return value;
            }

            throw new Exception(InvalidTypeMessage(t));
        }
    }

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
