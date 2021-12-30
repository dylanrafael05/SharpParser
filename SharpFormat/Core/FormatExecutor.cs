using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpFormat
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

        public static FormatExecutor FromText(string text, FormatReader reader)
        {
            return new FormatExecutor(new FormatLexer(text).Lex(), reader);
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
        private Dictionary<string, EvaluableItem> ParseCommand()
        {
            var open = Require(
                TokenType.OpenCall,
                tok => $"Expected opening brace '(', but got {tok.representation}"
            );

            var args = new Dictionary<string, EvaluableItem>();
            while (Current.type != TokenType.CloseCall)
            {
                var argIden = Require(
                    TokenType.Id,
                    tok => $"Expected identifier for command argument, but got {tok.representation}"
                );

                var eq = Require(
                    TokenType.Equals,
                    tok => $"Expected equals sign after command argument name, but got {tok.representation}"
                );

                var item = Require(
                    TokenTypeHelpers.IsValue,
                    tok => $"Expected value after equals sign '=' for command argument, but got {tok.representation}"
                );

                args.Add((string)argIden.value.Value, item.value);

                if (Current.type == TokenType.Comma)
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            var close = Require(
                TokenType.CloseCall,
                tok => $"Expected closing brace ')', but got {tok.representation}"
            );

            return args;
        }
        private EvaluableItem ParseSet()
        {
            var eq = Require(
                TokenType.Equals,
                tok => $"Expected equals sign '=', but got {tok.representation}"
            );

            var item = Require(
                TokenTypeHelpers.IsValue,
                tok => $"Expected value after equals sign '=', but got {tok.representation}"
            );

            return item.value;
        }

        public void Execute()
        {
            // Main loop, executes while there are commands to execute
            while (Current.type != TokenType.EOS)
            {
                var identifier = Require(
                    TokenType.Id,
                    tok => $"Expected identifier for expression, but got {tok.representation}"
                );

                var idName = (string)identifier.value.Value;

                if (Current.type == TokenType.Equals)
                {
                    var eval = ParseSet();

                    if (!reader.info.setCommands.ContainsKey(idName))
                    {
                        throw new Exception($"Unrecognized set command '{idName}'");
                    }

                    reader.info.setCommands[idName](reader, eval);
                }
                else
                {
                    var args = ParseCommand();

                    if (!reader.info.commands.ContainsKey(idName))
                    {
                        throw new Exception($"Unrecognized command '{idName}'");
                    }

                    reader.info.commands[idName](reader, args);
                }
            }
        }
    }
}
