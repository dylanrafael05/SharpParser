using SharpFormat;
using System;
using System.Linq;

public class Program
{
    public static void Main(string[] args)
    {
        FormatReader.PreRegisterReaderType<TestReader>();
        var reader = new TestReader();
        reader.Evaluate(
@"
Pos := Add({0,0},Add(b={1,1},a={2,2}))
Print(""Hello"")
PrintArr({""Hello"",""Hello"",""Hello"",""Hello"",""Hello"",0,0,0,0,0})
");

        Console.WriteLine(reader.Pos);
    }
}
