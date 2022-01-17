using System;

namespace SharpParser.Model
{
    /// <summary>
    /// A basic unit of the format reader format
    /// </summary>
    internal class Token : SourceValue
    {
        public TokenType type;
        public string representation;
        public LiteralValueInterface value;

        public Token(object value, SourceLocation location) : base(location)
        {
            this.value = new LiteralValueInterface(value);
        }

        public Token(SourceLocation location) : base(location)
        {
            value = LiteralValueInterface.Void;
        }

        public override ValueInterface Evaluate(FormatReader reader) => value;

        public override string ToString()
            => $"{representation} {base.ToString()}";
    }
}
