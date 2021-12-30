using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpFormat
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns whether or not a type is supported by this class.
        /// </summary>
        public static bool TypeIsSupported(Type t)
        {
            return (
                t == typeof(string) ||
                t == typeof(float) ||
                t == typeof(int) ||
                t == typeof(bool) ||
                t == typeof(object) ||
                t.IsEnum ||
                (Nullable.GetUnderlyingType(t) is var ut && ut != null && TypeIsSupported(ut))
            );
        }

        /// <summary>
        /// Get the name of the given type.
        /// Throws <see cref="Exception"/> if the provided type is unsupported.
        /// </summary>
        public static string TypeName(Type t)
        {
            if (t == typeof(string))
            {
                return "string";
            }
            else if (t == typeof(float))
            {
                return "floating-point number";
            }
            else if (t == typeof(int))
            {
                return "integer";
            }
            else if (t == typeof(bool))
            {
                return "boolean";
            }
            else if (t == typeof(object))
            {
                return "anything";
            }
            else if (t.IsEnum)
            {
                return $"enumeration '{t.Name}'";
            }
            else if (Nullable.GetUnderlyingType(t) is var ut && ut != null)
            {
                return $"nullable {TypeName(ut)}";
            }

            throw new Exception("Source code error: unsupported type.");
        }
    }

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
    }

    /// <summary>
    /// Helper functions for TokenType
    /// </summary>
    internal static class TokenTypeHelpers
    {
        public static bool IsValue(this TokenType tokenType)
            => tokenType == TokenType.Number || tokenType == TokenType.Id || tokenType == TokenType.String || tokenType == TokenType.NullBang;
    }

    /// <summary>
    /// A basic unit of the format reader format
    /// </summary>
    internal class Token
    {
        public TokenType type;
        public string representation;
        public LiteralItem value;

        public Token(object value)
        {
            this.value = new LiteralItem(value);
        }

        public Token()
        {
            value = null;
        }
    }
}
