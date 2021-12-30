using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpFormat
{
    /// <summary>
    /// The class responsible for breaking input text into <see cref="Token"/>s.
    /// </summary>
    internal class FormatLexer
    {
        private const char EOS = '\0';

        private int idx;
        private string text;

        public FormatLexer(string text)
        {
            this.text = text;
            idx = 0;
        }

        // A helper function to safely get the current char
        private char Get() => idx >= text.Length ? EOS : text[idx];

        // A helper function which skips to the next character while the predicate returns true
        private void SkipWhile(Predicate<char> pred)
        {
            while (pred(Get()) && Get() != EOS)
            {
                idx++;
            }
        }
        // A helper function which reads the text upto the first character where the predicate 
        // returns false
        private string ReadWhile(Predicate<char> pred)
        {
            var startIdx = idx;
            SkipWhile(pred);

            return text.Substring(startIdx, idx - startIdx);
        }

        // A helper function which reads a number
        private Token ReadNumber(bool isNegative)
        {
            var section = ReadWhile(c => char.IsDigit(c) || c == '.');

            object value;

            if (section.Contains("."))
            {
                if (!float.TryParse(section, out var valueFloat))
                {
                    throw new ArgumentException($"Invalid number: '{section}'.");
                }

                if (isNegative) valueFloat = -valueFloat;
                value = valueFloat;
            }
            else
            {
                if (!int.TryParse(section, out var valueInt))
                {
                    throw new ArgumentException($"Invalid integer: '{section}'.");
                }

                if (isNegative) valueInt = -valueInt;
                value = valueInt;
            }

            return new Token(value) { type = TokenType.Number, representation = $"'{section}'" };
        }

        public IEnumerator<Token> Lex()
        {
            // Run forever... (the resulting IEnumerable<Token> will "end" when a token with type EOS is returned)
            while (true)
            {
                switch (Get())
                {
                    // End of string
                    case EOS:
                        yield return new Token { type = TokenType.EOS, representation = "end of string" };
                        break;

                    // Control characters
                    case '(':
                        yield return new Token { type = TokenType.OpenCall, representation = "'('" };
                        idx++;
                        break;
                    case ')':
                        yield return new Token { type = TokenType.CloseCall, representation = "')'" };
                        idx++;
                        break;
                    case '{':
                        yield return new Token { type = TokenType.OpenObject, representation = "'{'" };
                        idx++;
                        break;
                    case '}':
                        yield return new Token { type = TokenType.CloseObject, representation = "'}'" };
                        idx++;
                        break;
                    case '=':
                        yield return new Token { type = TokenType.Equals, representation = "'='" };
                        idx++;
                        break;
                    case ',':
                        yield return new Token { type = TokenType.Comma, representation = "','" };
                        idx++;
                        break;
                    case '!':
                        yield return new Token(null) { type = TokenType.NullBang, representation = "'!'" };
                        idx++;
                        break;

                    // Negative number
                    case '-':
                        idx++;
                        yield return ReadNumber(true);
                        break;

                    // String
                    case '"':
                    {
                        idx++;
                        var section = ReadWhile(ch => ch != '"');

                        if (Get() == EOS)
                        {
                            throw new Exception("Unterminated string");
                        }

                        idx++;

                        yield return new Token(section) { type = TokenType.String, representation = $"'{section}'" };
                        break;
                    }

                    // Whitespace
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        SkipWhile(c => c == ' ' || c == '\t' || c == '\n' || c == '\r');
                        break;

                    // Other cases
                    default:
                    {
                        // Positive numbers
                        if (char.IsDigit(Get()))
                        {
                            yield return ReadNumber(false);
                        }
                        // Identifiers
                        else if (char.IsLetter(Get()) || Get() == '_')
                        {
                            var section = ReadWhile(c => char.IsLetterOrDigit(c) || c == '_');
                            yield return new Token(section) { type = TokenType.Id, representation = $"'{section}'" };
                        }
                        // Unrecognized characters
                        else
                        {
                            throw new Exception($"Unrecognized character '{Get()}'");
                        }

                        break;
                    }
                }
            }
        }
    }
}
