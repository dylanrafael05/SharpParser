using System;

namespace SharpParser.Helpers
{
    public class TupleTypeInfo
    {
        public Type Type { get; set; }
        public string Name { get; set; }

        public bool IsNamed => Name != null;
    }
}