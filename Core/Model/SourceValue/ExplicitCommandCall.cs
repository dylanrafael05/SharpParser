using System.Collections.Generic;
using System;

namespace SharpParser.Model
{
    internal class ExplicitCommandCall : CommandCall
    {
        public Dictionary<string, ValueInterface> args;

        public ExplicitCommandCall(string commandName, Dictionary<string, ValueInterface> args, SourceLocation location) : base(commandName, location)
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
                command.Explicit(reader, args);
                return ValueInterface.Void;
            }
            else
            {
                return command.Explicit(reader, args);
            }
        }
    }
}
