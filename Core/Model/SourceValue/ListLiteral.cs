namespace SharpParser.Model
{
    internal class ListLiteral : SourceValue
    {
        public object[] values;
        public ListLiteral(object[] values, SourceLocation location) : base(location)
        {
            this.values = values;
        }

        public override ValueInterface Evaluate(FormatReader reader)
            => new LiteralValueInterface(values);
    }
}
