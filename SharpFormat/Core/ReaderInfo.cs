using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpFormat
{
    /// <summary>
    /// The delegate representing a command.
    /// </summary>
    internal delegate ValueInterface Command(FormatReader reader, Dictionary<string, ValueInterface> args);

    /// <summary>
    /// The delegate representing a property command.
    /// </summary>
    internal delegate void PropertyCommand(FormatReader reader, ValueInterface value);

    /// <summary>
    /// The class responsible for carrying the underlying information of a <see cref="FormatReader"/>.
    /// </summary>
    internal class ReaderInfo
    {
        public Dictionary<string, (Command cmd, bool isVoid)> commands;
        public Dictionary<string, PropertyCommand> setCommands;

        public ReaderInfo()
        {
            commands = new Dictionary<string, (Command, bool)>();
            setCommands = new Dictionary<string, PropertyCommand>();
        }
    }
}
