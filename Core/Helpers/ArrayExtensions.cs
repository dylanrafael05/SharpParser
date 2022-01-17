using System;

namespace SharpParser.Helpers
{
    public static class ArrayExtensions
    {
        public static Type UnderlyingType(Type t)
        {
            if (t.IsArray)
                return t.GetElementType();

            return null;
        }
    }
}