using System.Collections.Generic;
using System.Reflection;

namespace SharpParser.Model
{
    /// <summary>
    /// The types which a token can be
    /// </summary>
    internal enum TokenType
    {
        Number,
        String,
        Id,
        OpenCall,
        Equals,
        Comma,
        CloseCall,
        NullBang,
        EOS,
        OpenObject,
        CloseObject,
        PropSet,
    }

    /// <summary>
    /// Helper functions for TokenType
    /// </summary>
    internal static class TokenTypeHelpers
    {
        public static bool IsValue(this TokenType tokenType)
            => tokenType == TokenType.Number || tokenType == TokenType.Id || tokenType == TokenType.String || tokenType == TokenType.NullBang;
    }
}
