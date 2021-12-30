namespace SharpFormat
{
    /// <summary>
    /// Represents anything which could be evaluated.
    /// </summary>
    internal interface IEvaluable
    {
        ValueInterface Evaluate(FormatReader reader);
    }
}
