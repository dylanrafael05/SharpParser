using SharpParser.Helpers;
using System;
using System.Linq;

namespace SharpParser.Model
{
    /// <summary>
    /// A value stored in its literal form.
    /// </summary>
    internal class LiteralValueInterface : ValueInterface
    {
        public LiteralValueInterface(object value, bool isVoid = false) : base(value, isVoid) { }

        public new static LiteralValueInterface Void => new LiteralValueInterface(null, true);

        public override object GetAsBoolInternal()
        {
            if (Value is string s && (s == "true" || s == "false"))
            {
                return s == "true";
            }

            throw new Exception(InvalidTypeMessage<bool>());
        }

        public override object GetAsEnumInternal(Type t)
        {
            if (Value is string s && EnumHelpers.TryParseFromType(t, s, out var value))
            {
                return value;
            }

            throw new Exception(InvalidTypeMessage(t));
        }

        public override object GetAsTupleInternal(Type t, TupleTypeInfo[] uts)
        {
            if (Value is object[] objs)
            {
                if (objs.Length != uts.Length)
                    throw new Exception(InvalidTypeMessage(t));

                //Console.WriteLine("[" + string.Join(", ", uts.Select(m => m.Type.FullName)) + "]");

                var results = new object[objs.Length];

                for(int i = 0; i < objs.Length; i++)
                {
                    results[i] = new LiteralValueInterface(objs[i]).GetterForType(uts[i].Type)();
                }

                return Helpers.ValueTupleExtensions.ToTuple(results);
            }

            throw new Exception(InvalidTypeMessage(t));
        }

        public override object GetAsArrayInternal(Type t, Type ut)
        {
            if(Value is object[] objs)
            {
                var results = Array.CreateInstance(ut, objs.Length);

                for(int i = 0; i < objs.Length; i++)
                {
                    results.SetValue(new LiteralValueInterface(objs[i]).GetterForType(ut)(), i);
                }

                return results;
            }

            throw new Exception(InvalidTypeMessage(t));
        }
    }
}
