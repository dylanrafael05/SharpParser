using SharpParser.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpParser
{
    /// <summary>
    /// The class responsible for breaking input text into <see cref="Token"/>s.
    /// </summary>
    internal class FormatLexer
    {
        private const char EOS = '\0';

        private int idx;
        private SourceLocation loc;
        private string text;

        public FormatLexer(string text, string sourceName)
        {
            this.text = text;
            idx = 0;
            loc = new SourceLocation
            {
                Line = 1,
                Chracter = 1,
                SourceName = sourceName
            };
        }

        // A helper function to safely get the current char
        private char Get() => idx >= text.Length ? EOS : text[idx];
        private void Advance(int count = 1)
        {
            idx += count;
            loc.Chracter += count;
        }

        // A helper function which skips to the next character while the predicate returns true
        private void SkipWhile(Predicate<char> pred)
        {
            while (pred(Get()) && Get() != EOS)
            {
                Advance();
            }
        }

        private void SkipLines()
        {
            while (Get() == '\n' || Get() == '\r')
            {
                if (Get() == '\r')
                {
                    Advance();
                    continue;
                }

                Advance();
                loc.Line++;
                loc.Chracter = 1;
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
                    throw new ArgumentException($"Invalid number: '{section}' ({loc})");
                }

                if (isNegative) valueFloat = -valueFloat;
                value = valueFloat;
            }
            else
            {
                if (!int.TryParse(section, out var valueInt))
                {
                    throw new ArgumentException($"Invalid integer: '{section}' ({loc})");
                }

                if (isNegative) valueInt = -valueInt;
                value = valueInt;
            }

            return new Token(value, loc) { type = TokenType.Number, representation = $"'{section}'" };
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
                        yield return new Token(loc) { type = TokenType.EOS, representation = "end of string" };
                        break;

                    // Control characters
                    case '(':
                        yield return new Token(loc) { type = TokenType.OpenCall, representation = "'('" };
                        Advance();
                        break;
                    case ')':
                        yield return new Token(loc) { type = TokenType.CloseCall, representation = "')'" };
                        Advance();
                        break;
                    case '{':
                        yield return new Token(loc) { type = TokenType.OpenObject, representation = "'{'" };
                        Advance();
                        break;
                    case '}':
                        yield return new Token(loc) { type = TokenType.CloseObject, representation = "'}'" };
                        Advance();
                        break;
                    case '=':
                        yield return new Token(loc) { type = TokenType.Equals, representation = "'='" };
                        Advance();
                        break;
                    case ',':
                        yield return new Token(loc) { type = TokenType.Comma, representation = "','" };
                        Advance();
                        break;
                    case '!':
                        yield return new Token(null, loc) { type = TokenType.NullBang, representation = "'!'" };
                        Advance();
                        break;
                    case ':':
                    {
                        Advance();
                        if (Get() != '=')
                        {
                            throw new Exception("Unrecognized character ':', did you mean ':='?");
                        }
                        yield return new Token(loc) { type = TokenType.PropSet, representation = "':='" };
                        Advance();

                        break;
                    }

                    // Negative number
                    case '-':
                        Advance();
                        yield return ReadNumber(true);
                        break;

                    // String
                    case '"':
                    {
                        Advance();
                        var section = ReadWhile(ch => ch != '"');

                        if (Get() == EOS)
                        {
                            throw new Exception("Unterminated string");
                        }

                        Advance();

                        yield return new Token(section, loc) { type = TokenType.String, representation = $"'{section}'" };
                        break;
                    }

                    // Whitespace
                    case ' ':
                    case '\t':
                        SkipWhile(c => c == ' ' || c == '\t');

                        break;
                    case '\n':
                    case '\r':
                        SkipLines();
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
                            yield return new Token(section, loc) { type = TokenType.Id, representation = $"'{section}'" };
                        }
                        // Unrecognized characters
                        else
                        {
                            throw new Exception($"Unrecognized character '{Get()}' ({loc})");
                        }

                        break;
                    }
                }
            }
        }
    }
}
