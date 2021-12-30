using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpFormat
{
    /// <summary>
    /// The delegate representing a command.
    /// </summary>
    internal delegate void Command(FormatReader reader, Dictionary<string, EvaluableItem> args);

    /// <summary>
    /// The delegate representing a property command.
    /// </summary>
    internal delegate void PropertyCommand(FormatReader reader, EvaluableItem value);

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
