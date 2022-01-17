using SharpParser.Helpers;
using System;
using static SharpParser.Helpers.TypeExtensions;

namespace SharpParser.Model
{
    /// <summary>
    /// A class which represents an object value, and how to convert it to given types.
    /// </summary>
    internal class ValueInterface
    {
        public virtual object Value { get; }
        public virtual bool IsVoid { get; }

        public ValueInterface(object value, bool isVoid = false)
        {
            Value = value;
            IsVoid = isVoid;
        }

        protected string InvalidTypeMessage<T>() => InvalidTypeMessage(typeof(T));
        protected string InvalidTypeMessage(Type t) => $"Invalid value for {TypeName(t)}: {Value}";

        public static ValueInterface Void => new ValueInterface(null, true);

        public virtual object GetAsStringInternal()
        {
            if (Value is string s)
            {
                return s;
            }

            throw new Exception(InvalidTypeMessage<string>());
        }
        public object GetAsString()
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsStringInternal();
        }

        public virtual object GetAsIntInternal()
        {
            if (Value is int i)
            {
                return i;
            }

            throw new Exception(InvalidTypeMessage<int>());
        }
        public object GetAsInt()
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsIntInternal();
        }

        public virtual object GetAsFloatInternal()
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
        public object GetAsFloat()
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsFloatInternal();
        }

        public virtual object GetAsBoolInternal()
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            if (Value is bool b)
            {
                return b;
            }

            throw new Exception(InvalidTypeMessage<bool>());
        }
        public virtual object GetAsBool()
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsBoolInternal();
        }


        public virtual object GetAsEnumInternal(Type t)
        {
            if (Value.GetType() == t)
            {
                return Value;
            }

            throw new Exception(InvalidTypeMessage(t));
        }
        public object GetAsEnum(Type t)
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsEnumInternal(t);
        }

        public virtual object GetAsNullableInternal(Type t, Type nullableUt)
            => Value is null ? Activator.CreateInstance(t) : Activator.CreateInstance(t, GetterForType(nullableUt)());
        public object GetAsNullable(Type t, Type ut)
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsNullableInternal(t, ut);
        }

        public virtual object GetAsTupleInternal(Type t, TupleTypeInfo[] uts)
        {
            if(Value.GetType() == t)
                return Value;

            throw new Exception(InvalidTypeMessage(t));
        }
        public object GetAsTuple(Type t, TupleTypeInfo[] uts)
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsTupleInternal(t, uts);
        }

        public virtual object GetAsArrayInternal(Type t, Type ut)
        {
            if(Value.GetType() == t)
                return Value;

            throw new Exception(InvalidTypeMessage(t));
        }
        public object GetAsArray(Type t, Type ut)
        {
            if(IsVoid)
                throw new Exception("Cannot access a void value.");

            return GetAsArrayInternal(t, ut);
        }

        public Func<object> GetterForType(Type t)
        {
            if (IsVoid)
                throw new Exception("Cannot access a void value.");

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
            else if (Nullable.GetUnderlyingType(t) is var nullableUt && nullableUt != null)
                return () => GetAsNullable(t, nullableUt);
            else if (ValueTupleExtensions.UnderlyingTypes(t) is var underlyingTypes && underlyingTypes != null)
                return () => GetAsTuple(t, underlyingTypes);
            else if (ArrayExtensions.UnderlyingType(t) is var arrayUt && arrayUt != null)
                return () => GetAsArray(t, arrayUt);

            throw new NotImplementedException();
        }
    }
}
