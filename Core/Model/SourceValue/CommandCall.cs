namespace SharpParser.Model
{
    internal abstract class CommandCall : SourceValue
    {
        public string commandName;
        public CommandCall(string commandName, SourceLocation location) : base(location)
        {
            this.commandName = commandName;
        }
    }
}
