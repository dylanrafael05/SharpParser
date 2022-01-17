using System;
using SharpFormat;

public class TestReader : FormatReader
{
    [FormatCommand]
    public (float x, float y) Add((float x, float y) a, (float x, float y) b)
    {
        return (a.x + b.x, a.y + b.y);
    }

    [FormatCommand]
    public void Print(object obj)
        => Console.WriteLine(obj);

    [FormatCommand]
    public void PrintArr(string[] obj)
        => Console.WriteLine(string.Join(", ", obj));

    [FormatProp]
    public (float x, float y) Pos { get; set; }
}
