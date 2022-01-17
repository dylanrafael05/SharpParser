namespace SharpParser.Model
{
    internal class PropertyCall : SourceValue
    {
        public string propertyName;
        public ValueInterface value;

        public PropertyCall(string propertyName, ValueInterface value, SourceLocation location) : base(location)
        {
            this.propertyName = propertyName;
            this.value = value;
        }

        public override ValueInterface Evaluate(FormatReader reader)
        {
            return reader.info.setCommands[propertyName](reader, value);
        }
    }
}
