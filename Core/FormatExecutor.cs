using SharpParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpParser
{
    /// <summary>
    /// The class responsible for executing commands from a stream of <see cref="Token"/>s.
    /// </summary>
    internal class FormatExecutor
    {
        private IEnumerator<Token> tokenStream;
        private FormatReader reader;

        public FormatExecutor(IEnumerator<Token> tokenStream, FormatReader reader)
        {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
            this.reader = reader;
        }

        public static FormatExecutor FromText(string text, string sourceName, FormatReader reader)
        {
            return new FormatExecutor(new FormatLexer(text, sourceName).Lex(), reader);
        }

        private Token Current => tokenStream.Current;
        private void Advance()
        {
            tokenStream.MoveNext();
        }

        // Helper function to read one token
        private Token Read()
        {
            var t = Current;
            Advance();
            return t;
        }
        
        // Helper functions to read a token and check its type
        private Token Require(TokenType type, Func<Token, string> errorMessage)
        {
            var t = Read();

            if (t.type != type)
            {
                throw new Exception(errorMessage(t));
            }

            return t;
        }
        private Token Require(Predicate<TokenType> typePred, Func<Token, string> errorMessage)
        {
            var t = Read();

            if (!typePred(t.type))
            {
                throw new Exception(errorMessage(t));
            }

            return t;
        }

        // Parse one command
        private CommandCall ParseCommand(string id, SourceLocation location)
        {
            var open = Require(
                TokenType.OpenCall,
                tok => $"Expected opening brace '(', but got {tok}"
            );

            if(Current.type == TokenType.CloseCall)
            {
                return new NonExplicitCommandCall(id, new ValueInterface[0], location);
            }
            else if(Current.type == TokenType.Id)
            {
                var iden = Read();
                if(Current.type == TokenType.Equals)
                {
                    Read();
                    return ParseExplicitCommandCall(id, (string)iden.value.GetAsString(), location);
                }
                else
                {
                    return ParseNonExplicitCommandCall(id, iden.Evaluate(reader), location);
                }
            }
            else
            {
                return ParseNonExplicitCommandCall(id, EvaluateValue(), location);
            }
        }
        private ExplicitCommandCall ParseExplicitCommandCall(string id, string firstArgName, SourceLocation location)
        {
            var args = ReadValueDictionary(TokenType.CloseCall, firstArgName);

            var close = Require(
                TokenType.CloseCall,
                tok => $"Expected closing brace ')', but got {tok}"
            );

            return new ExplicitCommandCall(id, args, location);
        }
        private NonExplicitCommandCall ParseNonExplicitCommandCall(string id, ValueInterface firstArg, SourceLocation location)
        {
            var args = ReadValueList(TokenType.CloseCall, firstArg);

            var close = Require(
                TokenType.CloseCall,
                tok => $"Expected closing brace ')', but got {tok}"
            );

            return new NonExplicitCommandCall(id, args.ToArray(), location);
        }

        private Dictionary<string, ValueInterface> ReadValueDictionary(TokenType ender, string firstArgName = null)
        {
            var vals = new Dictionary<string, ValueInterface>();

            if (firstArgName != null)
            {
                vals.Add(firstArgName, EvaluateValue());
                if (Current.type != TokenType.Comma)
                {
                    return vals;
                }
                else
                {
                    Advance();
                }
            }

            while (Current.type != ender)
            {
                var argIden = Require(
                    TokenType.Id,
                    tok => $"Expected identifier for command argument, but got {tok}"
                );

                var eq = Require(
                    TokenType.Equals,
                    tok => $"Expected equals sign '=' after command argument name, but got {tok}"
                );

                var item = EvaluateValue();

                vals.Add((string)argIden.value.Value, item);

                if (Current.type == TokenType.Comma)
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            return vals;
        }
        private List<ValueInterface> ReadValueList(TokenType ender, ValueInterface first = null)
        {
            var vals = new List<ValueInterface>();

            if (first is object)
            {
                vals.Add(first);
                if (Current.type != TokenType.Comma)
                {
                    return vals;
                }
                else
                {
                    Advance();
                }
            }

            while (Current.type != ender)
            {
                var item = EvaluateValue();
                vals.Add(item);

                if (Current.type == TokenType.Comma)
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            return vals;
        }
        private PropertyCall ParsePropertyCall(string id, SourceLocation startLoc)
        {
            var eq = Require(
                TokenType.PropSet,
                tok => $"Expected set symbol ':=', but got {tok}"
            );

            var item = EvaluateValue();

            return new PropertyCall(id, item, startLoc);
        }
        private ListLiteral ParseListLiteral()
        {
            var open = Require(
                TokenType.OpenObject,
                tok => $"Expected open object symbol '{{' but got {tok}"
            );

            var vals = ReadValueList(TokenType.CloseObject);

            var close = Require(
                TokenType.CloseObject,
                tok => $"Expected close object symbol '}}' but got {tok}"
            );

            return new ListLiteral(vals.Select(v => v.Value).ToArray(), open.Location);
        }

        private ValueInterface EvaluateValue()
        {
            if(Current.type == TokenType.Id)
            {
                var id = Read();
                if (Current.type == TokenType.OpenCall)
                {
                    return ParseCommand((string)id.value.GetAsString(), id.Location).Evaluate(reader);
                }
                else if(Current.type == TokenType.PropSet)
                {
                    return ParsePropertyCall((string)id.value.GetAsString(), id.Location).Evaluate(reader);
                }

                return id.Evaluate(reader);
            }
            else if(Current.type == TokenType.OpenObject)
            {
                return ParseListLiteral().Evaluate(reader);
            }
            else if(TokenTypeHelpers.IsValue(Current.type))
            {
                var tok = Read();
                var val = tok.Evaluate(reader);

                return val;
            }

            throw new Exception($"Invalid value {Current}");
        }

        public void Execute()
        {
            // Main loop, executes while there are commands to execute
            while (Current.type != TokenType.EOS)
            {
                EvaluateValue();
            }
        }
    }
}
