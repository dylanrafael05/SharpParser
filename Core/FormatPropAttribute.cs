using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpParser
{
    /// <summary>
    /// The attribute which denotes that a given property of a direct subclass of <see cref="FormatReader"/> is a property command.
    /// If provided, <see cref="Name"/> determines the name of this command, otherwise it is the name of the method.
    /// The property must have a setter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatPropAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
