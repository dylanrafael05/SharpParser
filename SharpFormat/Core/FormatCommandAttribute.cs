using System;
using System.Reflection;

namespace SharpFormat
{
    /// <summary>
    /// The attribute which denotes that a given method of a direct subclass of <see cref="FormatReader"/> is a command.
    /// If provided, <see cref="Name"/> determines the name of this command, otherwise it is the name of the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class FormatCommandAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
