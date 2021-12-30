using System;
using SharpFormat;

/// <summary>
/// An example of how to use the <see cref="FormatReader"/> API.
/// </summary>
public class ExampleReader : FormatReader
{
    /// <summary>
    /// Not necessary to make this private, just prevents this reader from being used in production code.
    /// </summary>
    private ExampleReader() : base()
    {}

    /// <summary>
    /// A simple command, defined using the [FormatCommand] attribute.
    /// Can be called from the input using 'COMMAND(ARG=VALUE)' syntax.
    /// </summary>
    [FormatCommand]
    public void HelloWorld()
    {
        Console.WriteLine("Hello World!");
    }

    /// <summary>
    /// A different command, this time with a parameter.
    /// Commands can take any number of parameters given they have names which are
    /// valid identifiers in the context of a format reader.
    /// 
    /// The supported value types are bool, int, float, string, any enum, bool?, int?, float?, any enum?, and object.
    /// </summary>
    [FormatCommand]
    public void Hello(string name)
    {
        Console.WriteLine("Hello, " + name + "!");
    }

    /// <summary>
    /// Parameters may also have default arguments.
    /// </summary>
    /// <param name="x"></param>
    [FormatCommand]
    public void PrintNum(float? x = null)
    {
        Console.WriteLine(x);
    }

    /// <summary>
    /// A format property can be set from within the input using 'NAME = VALUE' syntax
    /// </summary>
    [FormatProp]
    public int Offset { get; set; }
}