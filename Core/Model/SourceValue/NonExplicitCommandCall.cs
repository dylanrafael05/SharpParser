using System;

namespace SharpParser.Model
{
    internal class NonExplicitCommandCall : CommandCall
    {
        public ValueInterface[] args;

        public NonExplicitCommandCall(string commandName, ValueInterface[] args, SourceLocation location) : base(commandName, location)
        {
            this.args = args;
        }

        public override ValueInterface Evaluate(FormatReader reader)
        {
            if(!reader.info.commands.ContainsKey(commandName))
                throw new Exception($"Command {commandName} not found {this}");

            var command = reader.info.commands[commandName];

            if (command.IsVoid)
            {
                command.NonExplicit(reader, args);
                return ValueInterface.Void;
            }
            else
            {
                return command.NonExplicit(reader, args);
            }
        }
    }
}
