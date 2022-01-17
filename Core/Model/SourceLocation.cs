namespace SharpParser.Model
{
    internal class SourceLocation
    {
        public int Line { get; set; }
        public int Chracter { get; set; }
        public string SourceName { get; set; }

        public override string ToString()
            => $"{SourceName}:{Line}:{Chracter}";
    }
}
