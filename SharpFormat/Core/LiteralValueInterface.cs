using System;

namespace SharpFormat
{
    /// <summary>
    /// A value stored in its literal form.
    /// </summary>
    internal class LiteralValueInterface : ValueInterface
    {
        public LiteralValueInterface(object value) : base(value) { }

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
}
