namespace SharpParser.Model
{
    /// <summary>
    /// Represents anything which could be evaluated.
    /// </summary>
    internal abstract class SourceValue
    {
        protected SourceValue(SourceLocation location)
        {
            Location = location;
        }

        public SourceLocation Location { get; set; }

        public abstract ValueInterface Evaluate(FormatReader reader);

        public override string ToString()
            => $"({Location})";
    }
}
