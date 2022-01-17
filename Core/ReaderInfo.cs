using SharpParser.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpParser
{
    /// <summary>
    /// The delegate representing an explicitly called command.
    /// </summary>
    internal delegate ValueInterface ExplicitCommand(FormatReader reader, Dictionary<string, ValueInterface> args);
    /// <summary>
    /// The delegate representing a non-explicitly called command.
    /// </summary>
    internal delegate ValueInterface NonExplicitCommand(FormatReader reader, ValueInterface[] args);

    /// <summary>
    /// The delegate representing a property command.
    /// </summary>
    internal delegate ValueInterface PropertyCommand(FormatReader reader, ValueInterface value);

    /// <summary>
    /// A class which holds information about a command.
    /// </summary>
    internal class Command
    {
        public ExplicitCommand Explicit { get; set; }
        public NonExplicitCommand NonExplicit { get; set; }
        public bool IsVoid { get; set; }
    }

    /// <summary>
    /// The class responsible for carrying the underlying information of a <see cref="FormatReader"/>.
    /// </summary>
    internal class ReaderInfo
    {
        public Dictionary<string, Command> commands;
        public Dictionary<string, PropertyCommand> setCommands;

        public ReaderInfo()
        {
            commands = new Dictionary<string, Command>();
            setCommands = new Dictionary<string, PropertyCommand>();
        }
    }
}
