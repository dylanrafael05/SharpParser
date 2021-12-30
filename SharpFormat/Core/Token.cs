using System.Collections.Generic;

namespace SharpFormat
{
    /// <summary>
    /// A basic unit of the format reader format
    /// </summary>
    internal class Token : IEvaluable
    {
        public TokenType type;
        public string representation;
        public LiteralValueInterface value;

        public Token(object value)
        {
            this.value = new LiteralValueInterface(value);
        }

        public Token()
        {
            value = null;
        }

        public ValueInterface Evaluate(FormatReader reader) => value;
    }

    internal class CommandCall : IEvaluable
    {
        public string command;
        public Dictionary<string, ValueInterface> args;

        public CommandCall(string command, Dictionary<string, ValueInterface> args)
        {
            this.command = command;
            this.args = args;
        }

        public ValueInterface Evaluate(FormatReader reader)
        {
            reader.info.commands[command].//...
        }
    }
}
